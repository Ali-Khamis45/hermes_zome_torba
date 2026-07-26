# Build context is the repository root — see docker-compose.yml and .github/workflows/release.yml.
# Uses Next.js's `output: "standalone"` (frontend/next.config.ts) to ship a minimal runtime image.

FROM node:26-alpine AS deps
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --legacy-peer-deps

FROM node:26-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY frontend/ ./
ARG NEXT_PUBLIC_API_URL=http://localhost:5080
ARG NEXT_PUBLIC_SIGNALR_URL=http://localhost:5080/hubs
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL \
    NEXT_PUBLIC_SIGNALR_URL=$NEXT_PUBLIC_SIGNALR_URL
RUN npm run build

FROM node:26-alpine AS runtime
WORKDIR /app

RUN addgroup --system hzt && adduser --system --ingroup hzt hzt
USER hzt

ENV NODE_ENV=production \
    PORT=3000 \
    HOSTNAME=0.0.0.0

COPY --from=build --chown=hzt:hzt /app/.next/standalone ./
COPY --from=build --chown=hzt:hzt /app/.next/static ./.next/static
COPY --from=build --chown=hzt:hzt /app/public ./public

EXPOSE 3000
CMD ["node", "server.js"]
