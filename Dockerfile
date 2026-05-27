# escape=`
# ──────────────────────────────────────────────────────────────────────────────
# Tebex-SpaceEngineers build image
#
# Requires Windows containers (not Linux containers).
# On Docker Desktop: Settings → "Switch to Windows containers..."
# On a Windows host/CI: docker buildx build --platform windows/amd64 .
#
# Build:
#   docker build -t tebex-se-build .
#
# Extract the DLL after building:
#   docker create --name se-build tebex-se-build
#   docker cp se-build:C:\output\Tebex-SpaceEngineers.dll .
#   docker rm se-build
# ──────────────────────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build

WORKDIR C:\build

# Copy solution and dependencies
COPY Tebex-SpaceEngineers.sln .
COPY Lib\ Lib\
COPY Tebex-SpaceEngineers\ Tebex-SpaceEngineers\

# Compile Release|x64 (no NuGet restore needed — all refs are local DLLs)
RUN msbuild Tebex-SpaceEngineers.sln `
        /p:Configuration=Release `
        /p:Platform=x64 `
        /nologo `
        /verbosity:minimal

# ── Output stage: strip the SDK layer, keep only the compiled DLL ─────────────
FROM mcr.microsoft.com/windows/servercore:ltsc2022

WORKDIR C:\output

COPY --from=build C:\build\Tebex-SpaceEngineers\bin\x64\Release\Tebex-SpaceEngineers.dll .
