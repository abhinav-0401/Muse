import express from "express";
import ContentController from "../Controllers/ContentController.js";
import { MongoPostRepo } from "../Repositories/PostRepo.js";

const contentRouter = express.Router();

const contentController = new ContentController(new MongoPostRepo());
contentRouter.post("/content/create", (req, res) =>
  contentController.createPost(req, res)
);
contentRouter.get("/content/all", (req, res) =>
  contentController.getAllPosts(req, res)
);
contentRouter.delete("/content/:id", (req, res) =>
  contentController.deletePost(req, res)
);

export default contentRouter;
