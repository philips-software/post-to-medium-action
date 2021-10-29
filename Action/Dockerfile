# Set the base image as the .NET 3.1 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:3.1 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./Action/PostMediumGitHubAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Brend Smits <brend.smits@philips.com>"
LABEL repository="https://github.com/philips-software/post-to-medium-action"
LABEL homepage="https://github.com/philips-software/post-to-medium-action"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:3.1
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/PostMediumGitHubAction.dll" ]