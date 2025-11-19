FROM node:20.18-alpine AS deps
WORKDIR /app

# Install a specific pnpm version globally to avoid signature validation issues.
RUN npm install -g pnpm@9.12.1
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

FROM node:20.18-alpine AS build
WORKDIR /app

RUN npm install -g pnpm@9.12.1
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Accept an API url at build time so the frontend can call the backend.
ARG VITE_API_URL=http://localhost:5117
ENV VITE_API_URL=$VITE_API_URL

RUN pnpm build

FROM nginx:1.27-alpine AS final

# Copy the compiled assets into nginx's html directory.
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]