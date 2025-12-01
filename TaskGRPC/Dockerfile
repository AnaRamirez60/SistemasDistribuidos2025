# Dockerfile
FROM node:20-alpine

WORKDIR /app
COPY package.json /app/
RUN npm install --omit=dev || (npm install mongodb@6 --omit=dev)
COPY proto /app/proto
COPY server /app/server

EXPOSE 8086
ENV GRPC_PORT=8086

WORKDIR /app/server
CMD ["node", "index.js"]
