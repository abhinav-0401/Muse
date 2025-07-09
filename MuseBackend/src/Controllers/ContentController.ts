import { Post, IPostRepo } from "../Repositories/PostRepo.js";
import type { Request, Response } from "express";
import jwt from "jsonwebtoken";
import type { DecodedPayload } from "./AuthController.js";

export default class ContentController {
  constructor(private _postRepo: IPostRepo) {}

  public async createPost(req: Request, res: Response) {
    const accessToken = req.get("Authorization")?.split(" ")[1];
    if (!accessToken) {
      res.sendStatus(400);
      return;
    }

    console.log("createPost: ", accessToken);

    const decoded = await this.verifyAccessToken(accessToken);
    if (!decoded) {
      res.sendStatus(400);
      return;
    }

    let post: Post | null = {
      content: req.body.content as string,
      userid: decoded.id,
      username: decoded.username,
    };

    post = await this._postRepo.createPost(post);
    if (!post) {
      res.status(500).send("couldn't create post");
      return;
    }

    console.log("Post created: ", post);
    res.status(200).json(post);
  }

  public async getAllPosts(req: Request, res: Response) {
    const accessToken = req.get("Authorization")?.split(" ")[1];
    if (!accessToken) {
      res.sendStatus(400);
      return;
    }

    const decoded = await this.verifyAccessToken(accessToken);
    if (!decoded) {
      res.sendStatus(400);
      return;
    }

    let posts = await this._postRepo.findAllByUserid(decoded.id);
    if (!posts) {
      res.sendStatus(500);
      return;
    }
    posts = posts.map((post) => {
      return {
        id: post._id?.toString(),
        username: post.username,
        userid: post.userid,
        content: post.content,
      };
    });
    console.log("posts", posts);
    res.json(posts);
  }

  private verifyAccessToken(token: string): Promise<DecodedPayload | null> {
    return new Promise((resolve) => {
      jwt.verify(
        token,
        process.env.ACCESS_TOKEN_SECRET as string,
        (err, decode) => {
          if (err || !decode) {
            return resolve(null);
          }

          resolve(decode as DecodedPayload);
        }
      );
    });
  }
}
