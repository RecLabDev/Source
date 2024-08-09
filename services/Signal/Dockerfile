FROM denoland/deno

USER deno

WORKDIR /app

COPY . .

EXPOSE 9339

RUN deno cache src/server.ts

CMD ["run", "-A", "src/server.ts"]