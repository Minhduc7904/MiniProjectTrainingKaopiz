using BuildingBlocks.Contracts.Api;

namespace BuildingBlocks.Presentation.Actors;

internal sealed class ActorHeaderException(string errorCode, string safeMessage, int statusCode)
    : ApiException(errorCode, safeMessage, statusCode);
