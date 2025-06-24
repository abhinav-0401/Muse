import express from "express";
import type { Express } from "express";

import authRouter from "./AuthRouter.js";

export default function assignRoutes(app: Express) {
  app.use(authRouter);
}
