ARG PROJECT_PATH

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT_PATH
WORKDIR /src

COPY . .
RUN dotnet restore "${PROJECT_PATH}"
RUN dotnet publish "${PROJECT_PATH}" --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG PROJECT_PATH
WORKDIR /app

RUN case "${PROJECT_PATH}" in \
      *MediaService.Worker*) \
        apt-get update \
        && apt-get install --yes --no-install-recommends ffmpeg poppler-utils \
        && rm -rf /var/lib/apt/lists/* ;; \
    esac

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet"]
