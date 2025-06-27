import express from "express";
import AuthController, { User } from "../Controllers/AuthControllers.js";
import { InMemoryDbService } from "../Services/DbService.js";

const authRouter = express.Router();

const authController = new AuthController(new InMemoryDbService<User>());

authRouter.post("/auth/signup", authController.signupController);
authRouter.post("/auth/login", authController.loginController);
authRouter.post("/auth/access-token", authController.accessTokenController);

export default authRouter;
