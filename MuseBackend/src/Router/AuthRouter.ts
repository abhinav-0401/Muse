import express from "express";
import AuthController from "../Controllers/AuthControllers.js";
import { MongoUserRepo } from "../Repositories/UserRepo.js";

const authRouter = express.Router();

const authController = new AuthController(new MongoUserRepo());

authRouter.post("/auth/signup", (req, res) =>
  authController.signupController(req, res)
);
authRouter.post("/auth/login", (req, res) =>
  authController.loginController(req, res)
);
authRouter.post("/auth/access-token", (req, res) =>
  authController.accessTokenController(req, res)
);
authRouter.get("/auth/user", (req, res) =>
  authController.userInfoController(req, res)
);

export default authRouter;
