using System;
using Root.Core;
using Root.Utils;
using Root.Errors;
using Root.Services.Interfaces;

namespace Root.Services;

/// <summary>
/// Coordinates synchronization operations between the Qargo and Master tenants.
/// Responsible for mapping, linking, and synchronizing resource unavailabilities.
/// </summary>
public class Service2Service : Base, IInteractService {

	/// <summary>
	/// Service instance representing the Qargo tenant.
	/// </summary>
	private readonly QargoService _qargo;

	/// <summary>
	/// Service instance representing the Master tenant.
	/// </summary>
	private readonly MasterService _master;


	/// <summary>
	/// Initializes a new instance of <see cref="Service2Service"/>
	/// with the required tenant service dependencies.
	/// </summary>
	/// <param name="qargo">
	/// The <see cref="QargoService"/> responsible for interacting with the Qargo tenant.
	/// </param>
	/// <param name="master">
	/// The <see cref="MasterService"/> responsible for interacting with the Master tenant.
	/// </param>
	public Service2Service(QargoService qargo, MasterService master) : base(
		name: "Service2Service") {
		_qargo = qargo;
		_master = master;
	}


	/// <summary>
	/// Links Qargo unavailabilities to their corresponding Master resource actions
	/// by matching shared resource IDs and assigning synchronization operations.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous linking operation.</returns>
	private async Task QargoToMaster() {
		Echo("Linking unavailabilities between qargo and master...");

		try {
			// ? Servive level resouce mapping
			await Task.WhenAll( // ? Parallel execution
				_qargo.MapResources(),
				_master.MapResources()
			);

			// ? Explore shared resources
			foreach (var (resourceName, resourceQargo) in _qargo.ResourceMap) {
				if (_master.ResourceMap.TryGetValue(resourceName, out var resourceMaster)) {

					// ? Link qargo resource's unavailabilities to master
					foreach (var u in resourceQargo.Unavails) {
						resourceMaster.Actions.Assign(u);
					}
				}
			}

			Echo("Successfully linked unavailabilities between qargo and master");
		}
		catch (Exception ex) {
			Echo("Failed to link unavailabilities between qargo and master");
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}


	/// <summary>
	/// Synchronizes unavailabilities between the Master and Qargo tenants.
	/// Existing unavailabilities are linked first, after which create and update
	/// operations are executed in parallel for each mapped resource.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous synchronization operation.</returns>
	public async Task SyncUnavailabilities() {
		try {
			await QargoToMaster();

			Echo("Syncing qargo and master...");

			// ? Explore master resources and execute unavailAction on qargo
			foreach (var (resourceName, resourceMaster) in _master.ResourceMap) {
				if (_qargo.ResourceMap.TryGetValue(resourceName, out var resouceQargo)) {

					var create = await _qargo.CreateUnavailabilitiesAsync(
						resourceId: resouceQargo.Id, 
						actions: resourceMaster.Actions
					);
					Echo("Created => " + JsonUtil.Prettify(create));

					var update = await _qargo.UpdateUnavailabilitiesAsync(
						resourceId: resouceQargo.Id, 
						actions: resourceMaster.Actions
					);
					Echo("Updated => " + JsonUtil.Prettify(update));
				}
			}

			Echo("Successfully synced qargo and master");
		}
		catch (Exception ex) {
			Echo("Failed to sync qargo and master");
			throw AppException.Label<AppException>(ex, Msg(ex.Message));
		}
	}
}
