using ClientService.Application.Dtos;

namespace ClientService.Application.Dtos;

/// <summary>
/// Friendly body for the logical-delete operation (F1 - Delete use case):
/// a human-readable message plus the deactivated client so the caller sees
/// the resulting state (Estado = false) without an extra GET.
/// </summary>
public sealed record ClienteDeleteResponse(string Message, ClienteResponse Cliente);