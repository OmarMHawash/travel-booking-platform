# 1. Builder Stage: Install dependencies and build the project
FROM node:22-alpine AS builder
WORKDIR /app

# Install dependencies
COPY package.json yarn.lock* ./
RUN yarn install --network-timeout 60000

# Copy the rest of the source code
COPY . .

# Run the build command
# The output: 'standalone' option in next.config.js will be used here
RUN yarn build

# 2. Runner Stage: Create the final, lean production image
FROM node:22-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
# Disable telemetry in the final image
ENV NEXT_TELEMETRY_DISABLED 1

# Create a non-root user for better security
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy the standalone output from the builder stage
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 8000

# The command to start the production server
CMD ["node", "server.js"]