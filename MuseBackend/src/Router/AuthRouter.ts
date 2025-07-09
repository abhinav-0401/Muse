import express from "express";
import AuthController from "../Controllers/AuthController.js";
import { MongoUserRepo } from "../Repositories/UserRepo.js";

const authRouter = express.Router();

const authController = new AuthController(new MongoUserRepo());

authRouter.post("/auth/signup", (req, res) => authController.signup(req, res));
authRouter.post("/auth/login", (req, res) => authController.login(req, res));
authRouter.get("/auth/access-token", (req, res) =>
  authController.generateAccessToken(req, res)
);
authRouter.get("/auth/user", (req, res) =>
  authController.getUserInfo(req, res)
);

export default authRouter;
