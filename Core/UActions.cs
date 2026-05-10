using System;
using Root.Errors;
using Root.Core.Interfaces;
using Root.DTOs.UnavailabilityListComponents;

namespace Root.Core;

/// <summary>
/// Represents a collection of unavailability synchronization actions.
/// Tracks which unavailabilities should be created and which should be updated
/// during tenant synchronization operations.
/// </summary>
public class UActions : IUActions {

	/// <summary>
	/// Stores unavailabilities that must be created,
	/// keyed by their external identifier.
	/// </summary>
	public readonly Dictionary<string, Unavailability> ToCreate;

	/// <summary>
	/// Stores unavailabilities that must be updated,
	/// keyed by their tenant-specific unavailability identifier.
	/// </summary>
	public readonly Dictionary<string, Unavailability> ToUpdate;


	/// <summary>
	/// Initializes a new instance of <see cref="UActions"/>
	/// with empty create and update action collections.
	/// </summary>
	public UActions() {
		ToCreate = [];
		ToUpdate = [];
	}


	/// <summary>
	/// Assigns an incoming <see cref="Unavailability"/> to the appropriate synchronization action.
	/// If a matching external ID exists in <see cref="ToCreate"/>, the entry is compared and,
	/// when differences are detected, moved into <see cref="ToUpdate"/>.
	/// </summary>
	/// <param name="unavail">
	/// The incoming unavailability instance to evaluate and assign.
	/// </param>
	/// <exception cref="ConfigException">
	/// Thrown when:
	/// <list type="bullet">
	/// <item><description><see cref="Unavailability.ExternalId"/> is <c>null</c>.</description></item>
	/// <item><description>The unavailability ID already exists in <see cref="ToUpdate"/>.</description></item>
	/// </list>
	/// </exception>
	public void Assign(Unavailability unavail) {

		// ? Validate input
		string externalId = unavail.ExternalId
			?? throw new ConfigException("Input unavailability external id must be not null");

		// ? Validate uniqueness
		string unavailId = unavail.Id;

		if (ToUpdate.ContainsKey(unavailId))
			throw new ConfigException("Input unavailability id must be unique");

		// ? Nothing to change
		if (!ToCreate.ContainsKey(externalId)) return;

		// ? If the existing unavailability differs
		// ? from the incoming one, mark it for update
		var a = unavail;
		var b = ToCreate[externalId];

		ToCreate.Remove(externalId); // ? Prepare to move unavailability

		if (a.Reason      != b.Reason
		||  a.Description != b.Description
		||  a.StartTime   != b.StartTime
		||  a.EndTime     != b.EndTime) {
			
			// ? Move unavailability to update action
			b.ExternalId = a.ExternalId; // ? Ensure they have same external id
			ToUpdate.Add(unavailId, b); // ? Use unavail id of qargo =>
												 // ? Update request done in the qargo tenant by that key
		}
	}
}
