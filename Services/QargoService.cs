using System;
using Root.Core;
using Root.Source;
using Root.Errors;
using Root.Services.Interfaces;
using Root.DTOs.UnavailabilityListComponents;

namespace Root.Services;

/// <summary>
/// Represents the Qargo tenant service responsible for retrieving and mapping
/// resource unavailabilities that are already linked through external identifiers.
/// Inherits authenticated tenant API functionality from <see cref="Tenant"/>.
/// </summary>
public class QargoService : Tenant, IResourseMap {

	/// <summary>
	/// Maps resource Names to their associated IDs and unavailabilities
	/// </summary>
	public readonly Dictionary<string, (string Id, List<Unavailability> Unavails)> ResourceMap;


	/// <summary>
	/// Initializes a new instance of <see cref="QargoService"/>
	/// using credentials loaded from the provided <see cref="Context"/>.
	/// </summary>
	/// <param name="ctx">
	/// The shared application context containing environment variables
	/// and the reusable <see cref="HttpSource"/>.
	/// </param>
	public QargoService(Context ctx) : base(
		ctx: 			 ctx,
		name:        "Qargo",
		secretEnv:	 "QARGO_SECRET",
		clientIdEnv: "QARGO_CLIENT_ID"
	) {
		ResourceMap = [];
	}


	/// <summary>
	/// Retrieves all Qargo tenant resources and maps their unavailabilities
	/// by normalized resource name.
	/// </summary>
	/// <remarks>
	/// Resource IDs differ between environments, therefore resources are matched
	/// using their names. If multiple resources share the same name, the mapping
	/// is considered ambiguous and synchronization for that resource is skipped
	/// to prevent incorrect updates.
	/// 
	/// Only resources with unique names are added to <c>ResourceMap</c>.
	/// </remarks>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous mapping operation.
	/// </returns>
	/// <exception cref="AppException">
	/// Thrown when an unexpected error occurs during resource or unavailability retrieval.
	/// </exception>
	public async Task MapResources() {
		Echo("Mapping resources to unavailabilities...");

		try {
			// ? Get every resource's unavailability
			var resources = await GetResourcesAsync();
			DetermineAmbiguity(resources);

			foreach (var r in resources) {
				string name = Normalize(r.Name);

				// ? Resource already marked as ambiguous
				if (_ambiguousResources.Contains(name)) {
					Warn($"Multiple target resources found for '{name}'. Sync skipped due to ambiguous mapping.");
					continue;
				}

				// ? Map resource names to unavailabilities
				var unavails = await GetUnavailabilitiesAsync(r.Id);
				ResourceMap.Add(name, (r.Id, unavails));
			}
			
			Echo("Successfully mapped resources to unavailabilities");
		}
		catch (Exception ex) {
			Echo("Failed to map resources to unavailabilities");
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}
}
