# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore

# ── Test stage ────────────────────────────────────────────────────────────────
# The Docker socket is bind-mounted at build time so Testcontainers can spin up
# a real Postgres container on the host daemon during the test run.
FROM build AS test
ENV RYUK_DISABLED=true
# Ports exposed by Testcontainers are on the HOST, not inside this build
# container — this env var tells the Testcontainers client where to reach them.
ENV TESTCONTAINERS_HOST_OVERRIDE=host-gateway
RUN --mount=type=bind,source=/var/run/docker.sock,target=/var/run/docker.sock \
    dotnet test --no-restore --logger "console;verbosity=normal"

# ── Publish stage ─────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish src/MyApp/MyApp.csproj -c Release -o /app --no-restore

# ── Final runtime image ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MyApp.dll"]
