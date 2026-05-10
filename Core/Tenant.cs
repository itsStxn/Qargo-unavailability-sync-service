using System;
using Root.DTOs;
using Root.Errors;
using Root.Source;
using Root.Records;
using Root.Core.Interfaces;
using System.Net.Http.Json;
using Root.DTOs.ResourceListComponents;
using Root.DTOs.UnavailabilityListComponents;

namespace Root.Core;

/// <summary>
/// Represents a tenant context and provides operations for managing resources and their unavailabilities
/// via the authenticated API. Inherits logging utilities from <see cref="Base"/> and delegates
/// all HTTP communication to <see cref="MyAuthRequest"/>.
/// </summary>
public class Tenant : Base, ITenant {

	/// <summary>The authenticated HTTP client used for all API calls made by this tenant.</summary>
	private readonly MyAuthRequest _auth;

	/// <summary>Application context providing shared services.</summary>
	private readonly Context _ctx;

	/// <summary>
	/// A set of resource identifiers that are ambiguous or cannot be uniquely resolved within the tenant context.
	/// </summary>
	protected readonly HashSet<string> _ambiguousResources;


	/// <summary>
	/// Initializes a new instance of <see cref="Tenant"/> with the given credentials and HTTP client.
	/// </summary>
	/// <param name="name">The service name passed to <see cref="Base"/> for log message prefixing.</param>
	/// <param name="clientIdEnv">Name of the following envaronment variable: OAuth2 client ID for authentication.</param>
	/// <param name="secretEnv">Name of the following envaronment variable: OAuth2 client secret for authentication.</param>
	/// <param name="ctx">Application context providing shared services such as HTTP client and environment variable access.</param>
	public Tenant(string name, string clientIdEnv, string secretEnv, Context ctx) : base(name) {
		_ctx = ctx;
		_ambiguousResources = [];
		_auth = new MyAuthRequest(
			name: 	 name, 
			http: 	 ctx.Http,
			secret: 	 ctx.Env.Load(secretEnv), 
			clientId: ctx.Env.Load(clientIdEnv) 
		);
	}


	/// <summary>
	/// Determines and tracks ambiguous resources by identifying duplicate normalized names within the provided resource collection.
	/// </summary>
	/// <param name="resources">The list of resources to analyze for name ambiguities.</param>
	/// <remarks>
	/// This method normalizes each resource name and groups resources by their normalized names.
	/// Any group containing more than one resource is considered ambiguous and is added to the 
	/// <see cref="_ambiguousResources"/> collection. A warning is logged for each detected duplicate.
	/// </remarks>
	protected void DetermineAmbiguity(List<Resource> resources) {
		var grouped = resources
			.GroupBy(r => Normalize(r.Name));

		foreach (var group in grouped) {
			if (group.Count() > 1) {
				_ambiguousResources.Add(group.Key);
				Warn($"Duplicate resource name detected: {group.Key}");
			}
		}
	} 

	/// <summary>
	/// Retrieves all resources for this tenant by paginating through the API until no further cursor is returned.
	/// </summary>
	/// <returns>A <see cref="Task"/> resolving to a flat <see cref="List{T}"/> of all <see cref="Resource"/> items.</returns>
	/// <exception cref="AppException">Thrown if any error occurs during fetching, wrapping the original exception.</exception>
	public async Task<List<Resource>> GetResourcesAsync() {
		Echo("Getting resources...");

		var resources = new List<Resource>();
		string? next = null;
		ResourceList data;

		try {
			do {
				// ? Prepare uri
				var endpoint = "resources/resource";
				endpoint += next == null ? string.Empty : $"?cursor={next}";

				// ? Fetch resources
				data = await _auth.SendAsync<ResourceList>(() =>
					new HttpRequestMessage(HttpMethod.Get, endpoint));

				// ? Store and run until no cursor is found
				resources.AddRange(data.Items);
				next = data.NextCursor;
			}
			while (next != null && data.Items.Count > 0);

			Echo("Successfully got resources");
			return resources;
		}
		catch {
			Error("Failed to get resources");
			throw;
		}
	}

	/// <summary>
	/// Retrieves all unavailabilities for a given resource by paginating through the API,
	/// filtered to only include entries where both <c>StartTime</c> and <c>EndTime</c> fall within <c>_ctx.UFilters.Year</c>.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to fetch unavailabilities for.</param>
	/// <returns>A <see cref="Task"/> resolving to a flat <see cref="List{T}"/> of matching <see cref="Unavailability"/> items.</returns>
	/// <exception cref="AppException">Thrown if any error occurs during fetching, wrapping the original exception.</exception>
	public async Task<List<Unavailability>> GetUnavailabilitiesAsync(string resourceId) {
		Echo($"Getting unavailabilities for resource: {resourceId}...");

		var unavails = new List<Unavailability>();
		UnavailabilityList data;
		string? next = null;

		try {
			do {
				// ? Prepare uri
				var endpoint = $"resources/resource/{resourceId}/unavailability";
				endpoint += next == null ? string.Empty : $"?cursor={next}";

				// ? Fetch resources
				data = await _auth.SendAsync<UnavailabilityList>(() =>
					new HttpRequestMessage(HttpMethod.Get, endpoint));

				// ? Filter by year and store
				int year = int.Parse(_ctx.UFilters.Year);

				unavails.AddRange(
					data.Items.Where(u => {
						var start = DateTimeOffset.Parse(u.StartTime);
						var end = string.IsNullOrEmpty(u.EndTime)
							? (DateTimeOffset?)null
							: DateTimeOffset.Parse(u.EndTime);

						return start.Year == year &&
							(end == null || end.Value.Year == year);
					})
				);

				// ? Run until no cursor is found
				next = data.NextCursor;
			}
			while (next != null && data.Items.Count > 0);

			Echo($"Successfully got unavailabilities for resource: {resourceId}");
			return unavails;
		}
		catch {
			Error($"Failed to get unavailabilities for resource: {resourceId}");
			throw;
		}
	}

	/// <summary>
	/// Creates all unavailabilities in the provided <see cref="UActions.ToCreate"/> map
	/// for the specified resource.
	/// </summary>
	/// <remarks>
	/// Each creation request is executed independently to ensure that failures
	/// do not prevent remaining unavailabilities from being processed.
	///
	/// Individual failures are logged while synchronization continues.
	/// </remarks>
	/// <param name="resourceId">
	/// The ID of the resource whose unavailabilities should be created.
	/// </param>
	/// <param name="actions">
	/// The action container holding unavailabilities to create.
	/// </param>
	/// <returns>
	/// A <see cref="BatchActionResult"/> containing the total number of creation
	/// attempts along with success and failure counts.
	/// </returns>
	public async Task<BatchActionResult> CreateUnavailabilitiesAsync(
		string resourceId,
		UActions actions
	) {
		// ? Early exit
		if (actions.ToCreate.Count == 0) {
			Warn($"No unavailabilities to create for resource: {resourceId}");

			return new BatchActionResult {
				ResourceId = resourceId,
				Succeeded = 0,
				Failed = [],
				Total = 0
			};
		}

		Echo($"Creating unavailabilities for resource: {resourceId}...");

		int success = 0;
		var failed = new List<string>();

		foreach (var (unavailId, unavail) in actions.ToCreate) {
			try {
				await CreateUnavailabilityAsync(resourceId, unavail);
				success++;

				Echo($"Successfully created unavailability: {unavailId}");
			}
			catch (Exception ex) {
				failed.Add(unavailId);

				Error(
					$"Failed to create unavailability '{unavailId}' " +
					$"for resource '{resourceId}': {ex.Message}"
				);
			}
		}

		Echo($"Finished creating unavailabilities for resource: {resourceId}");

		return new BatchActionResult {
			ResourceId = resourceId,
			Total = actions.ToCreate.Count,
			Succeeded = success,
			Failed = failed
		};
	}

	/// <summary>
	/// Sends a single POST request to create one <see cref="Unavailability"/> entry for a given resource.
	/// </summary>
	/// <param name="resourceId">The ID of the resource to attach the unavailability to.</param>
	/// <param name="unavail">The <see cref="Unavailability"/> object to serialize and POST.</param>
	/// <returns>A <see cref="Task"/> resolving to the <see cref="HttpResponseMessage"/> from the API.</returns>
	private async Task<HttpResponseMessage> CreateUnavailabilityAsync(string resourceId, Unavailability unavail) {
		string endpoint = $"resources/resource/{resourceId}/unavailability";
		var res = await _auth.SendAsync<HttpResponseMessage>(() =>
			new(HttpMethod.Post, endpoint) {
				Content = JsonContent.Create(unavail)
		});

		return res;
	}

	/// <summary>
	/// Updates all unavailabilities in the provided <see cref="UActions.ToUpdate"/> map
	/// for the specified resource.
	/// </summary>
	/// <remarks>
	/// Each update request is executed independently to ensure that failures
	/// do not prevent remaining unavailabilities from being processed.
	///
	/// Individual failures are logged while synchronization continues.
	/// </remarks>
	/// <param name="resourceId">
	/// The ID of the resource whose unavailabilities should be updated.
	/// </param>
	/// <param name="actions">
	/// The action container holding unavailabilities to update.
	/// </param>
	/// <returns>
	/// A <see cref="BatchActionResult"/> containing the total number of update
	/// attempts along with success and failure counts.
	/// </returns>
	public async Task<BatchActionResult> UpdateUnavailabilitiesAsync(
		string resourceId,
		UActions actions
	) {
		// ? Early exit
		if (actions.ToUpdate.Count == 0) {
			Warn($"No unavailabilities to update for resource: {resourceId}");

			return new BatchActionResult {
				ResourceId = resourceId,
				Succeeded = 0,
				Failed = [],
				Total = 0
			};
		}

		Echo($"Updating unavailabilities for resource: {resourceId}...");

		int success = 0;
		var failed = new List<string>();

		foreach (var (unavailId, newUnavail) in actions.ToUpdate) {
			try {
				await UpdateUnavailabilityAsync(resourceId, unavailId, newUnavail);
				success++;

				Echo($"Successfully updated unavailability: {unavailId}");
			}
			catch (Exception ex) {
				failed.Add(unavailId);

				Error(
					$"Failed to update unavailability '{unavailId}' " +
					$"for resource '{resourceId}': {ex.Message}"
				);
			}
		}

		Echo($"Finished updating unavailabilities for resource: {resourceId}");

		return new BatchActionResult {
			ResourceId = resourceId,
			Total = actions.ToUpdate.Count,
			Succeeded = success,
			Failed = failed
		};
	}

	/// <summary>
	/// Sends a single PUT request to replace one <see cref="Unavailability"/> entry for a given resource.
	/// </summary>
	/// <param name="resourceId">The ID of the resource the unavailability belongs to.</param>
	/// <param name="unavailId">The ID of the unavailability entry to replace.</param>
	/// <param name="newUnavail">The updated <see cref="Unavailability"/> object to serialize and PUT.</param>
	/// <returns>A <see cref="Task"/> resolving to the <see cref="HttpResponseMessage"/> from the API.</returns>
	private async Task<HttpResponseMessage> UpdateUnavailabilityAsync(string resourceId, string unavailId, Unavailability newUnavail) {
		string endpoint = $"resources/resource/{resourceId}/unavailability/{unavailId}";
		var res = await _auth.SendAsync<HttpResponseMessage>(() =>
			new(HttpMethod.Put, endpoint) {
				Content = JsonContent.Create(newUnavail)
		});

		return res;
	}
}
