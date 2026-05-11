using System;
using Root.Core;
using Root.Source;
using Root.Errors;
using Root.Services.Interfaces;

namespace Root.Services;

/// <summary>
/// Represents the master tenant service responsible for building
/// a resource-to-unavailability action map used for synchronization operations.
/// Inherits all authenticated tenant API functionality from <see cref="Tenant"/>.
/// </summary>
public class MasterService : Tenant, IResourseMap {

	/// <summary>
	/// Maps resource Names to their associated Ids and unavailability actions.
	/// Each entry contains the create/update operations required for synchronization.
	/// </summary>
	public readonly Dictionary<string, (string Id, UActions Actions)> ResourceMap;


	/// <summary>
	/// Initializes a new instance of <see cref="MasterService"/>
	/// using credentials loaded from the provided <see cref="Context"/>.
	/// </summary>
	/// <param name="ctx">
	/// The shared application context containing environment variables
	/// and the reusable <see cref="HttpSource"/>.
	/// </param>
	public MasterService(Context ctx) : base(
		ctx: 			 ctx,
		name:     	 "Master",
		secretEnv:   "MASTER_SECRET",
		clientIdEnv: "MASTER_CLIENT_ID"
	) {
		ResourceMap = [];
	}


	/// <summary>
	/// Retrieves all master tenant resources and maps their existing unavailabilities
	/// into <see cref="ResourceMap"/> as create actions.
	/// </summary>
	/// <remarks>
	/// Resource IDs differ between environments, therefore resources are matched
	/// using their names.
	///
	/// Existing unavailabilities from the master tenant are assumed to require
	/// creation on downstream tenants and are stored as <c>ToCreate</c> actions.
	///
	/// If multiple resources share the same name, the mapping is considered
	/// ambiguous and synchronization for that resource is skipped to prevent
	/// incorrect updates.
	///
	/// Only resources with unique names and at least one unavailability are added
	/// to <see cref="ResourceMap"/>.
	/// </remarks>
	/// <returns>
	/// A <see cref="Task"/> representing the asynchronous mapping operation.
	/// </returns>
	/// <exception cref="AppException">
	/// Thrown when an unexpected error occurs during resource or unavailability retrieval.
	/// </exception>
	public async Task MapResources() {
		Echo("Mapping resources to unavailability actions...");

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

				// ? Assume unavailabilities need to be created, not updated
				var unavails = await GetUnavailabilitiesAsync(r.Id);

				if (unavails.Count > 0) {
					var actions = new UActions();

					foreach (var u in unavails) {
						u.ExternalId = u.Id; // ? Assign external id to prepare 
													// ? for create operation in UAction
						actions.ToCreate.Add(u.Id, u);
					}

					// ? Map names to actions
					ResourceMap.Add(name, (r.Id, actions));
				}
			}

			Echo("Successfully mapped resources to unavailability actions...");
		}
		catch (Exception ex) {
			Echo("Failed to map resources to unavailability actions...");
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}
}
