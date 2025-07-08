import type { Request, Response } from "express";
import "dotenv/config";
import jwt from "jsonwebtoken";
import { IUserRepo, User } from "../Repositories/UserRepo.js";
import { Types } from "mongoose";

interface DecodedPayload {
  username: string;
  id: string;
}

interface AuthResponse {
  id: Types.ObjectId;
  username: string;
  accessToken: string;
  refreshToken: string;
}

export default class AuthController {
  constructor(private _userRepo: IUserRepo) {}

  public async signupController(req: Request, res: Response): Promise<void> {
    const username: string = req.body.username;
    const password: string = req.body.password;

    if (await this._userRepo.findByUsername(username)) {
      res.sendStatus(409);
      return;
    }

    const user: User = { username, password };
    const result = await this._userRepo.createUser(user);
    if (!result) {
      res.sendStatus(500);
      return;
    }

    // give access and refresh tokens
    const refreshToken = getRefreshToken(result);
    const accessToken = (await getAccessToken(refreshToken)) as string;

    res.status(200).json({
      id: result._id,
      username,
      accessToken,
      refreshToken,
    } as AuthResponse);
  }

  public async loginController(req: Request, res: Response): Promise<void> {
    const username: string = req.body.username;
    const password: string = req.body.password;

    const user = await this._userRepo.findByUsername(username);
    if (!user) {
      res.sendStatus(404);
      return;
    }

    if (password !== user?.password) {
      res.status(404).send("Incorrect password");
      return;
    } else if (password === user?.password) {
      const refreshToken = getRefreshToken(user);
      const accessToken = await getAccessToken(refreshToken);

      res.status(200).json({
        id: user._id,
        username,
        accessToken,
        refreshToken,
      } as AuthResponse);
      return;
    }
  }

  public async userInfoController(req: Request, res: Response): Promise<void> {
    console.log(req.get("Authentication"));
    const accessToken = req.get("Authentication")?.split(" ")[1];
    if (!accessToken) {
      res.sendStatus(400);
      return;
    }

    const decoded = await this.verifyAccessToken(accessToken);
    if (!decoded) {
      res.sendStatus(400);
      return;
    }

    const user = await this._userRepo.findById(decoded.id);
    if (!user) {
      res.send("couldn't find user");
      return;
    }

    res.status(200).json({ username: user.username });
  }

  public async accessTokenController(
    req: Request,
    res: Response
  ): Promise<void> {
    const refreshToken = req.get("Authorization")?.split(" ")[1];
    if (!refreshToken) {
      res.sendStatus(400);
      return;
    }
    const accessToken = await getAccessToken(refreshToken);

    if (!accessToken) {
      res.status(403).send("Invalid refresh token");
      return;
    }

    res.status(200).json({ accessToken });
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

export function getRefreshToken(user: User): string {
  return jwt.sign(
    { username: user.username, id: user._id?.toString() },
    process.env.REFRESH_TOKEN_SECRET as string,
    {
      expiresIn: "30d",
    }
  );
}

export function getAccessToken(refreshToken: string): Promise<string | null> {
  return new Promise((resolve) => {
    jwt.verify(
      refreshToken,
      process.env.REFRESH_TOKEN_SECRET as string,
      (err, decode) => {
        if (err || !decode) {
          console.error(err, decode);
          return resolve(null);
        }

        const user = decode as DecodedPayload;
        const accessToken = jwt.sign(
          { username: user.username, id: user.id },
          process.env.ACCESS_TOKEN_SECRET as string,
          { expiresIn: 20 }
        );
        resolve(accessToken);
      }
    );
  });
}
