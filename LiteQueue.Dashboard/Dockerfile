FROM node:22-alpine AS build
WORKDIR /app
COPY LiteQueue.Dashboard/package.json LiteQueue.Dashboard/package-lock.json* ./
RUN npm ci --legacy-peer-deps 2>/dev/null || npm install --legacy-peer-deps
COPY LiteQueue.Dashboard/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/litequeue-dashboard /usr/share/nginx/html
COPY LiteQueue.Dashboard/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
