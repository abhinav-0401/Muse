import type { Request, Response } from "express";
import "dotenv/config";
import jwt from "jsonwebtoken";
import { IDbService } from "../Services/DbService.js";

export interface User {
  username: string;
  password: string;
}

interface DecodedPayload {
  username: string;
}

// this is just for quick testing
const users = new Map<string, User>();
const numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];

export default class AuthController {
  private _dbService: IDbService<User>;

  constructor(dbService: IDbService<User>) {
    this._dbService = dbService;
  }

  async signupController(req: Request, res: Response): Promise<void> {
    const username: string = req.body.username;
    const password: string = req.body.password;

    console.log(req.cookies);

    if (this._dbService.has(username)) {
      res.sendStatus(409);
      return;
    }

    const user: User = { username, password };
    // users.set(username, user);
    this._dbService.set(username, user);

    // give access and refresh tokens
    const refreshToken = getRefreshToken(username);
    const accessToken = (await getAccessToken(refreshToken)) as string;

    res.status(200).json({ username, accessToken, refreshToken });
  }

  async loginController(req: Request, res: Response): Promise<void> {
    const username: string = req.body.username;
    const password: string = req.body.password;

    if (!this._dbService.has(username)) {
      res.sendStatus(404);
      return;
    }

    if (this._dbService.has(username)) {
      // const user = users.get(username);
      const user = this._dbService.get(username);
      if (password !== user?.password) {
        res.status(404).send("Incorrect password");
        return;
      } else if (password === user?.password) {
        const refreshToken = getRefreshToken(username);
        const accessToken = await getAccessToken(refreshToken);

        res.status(200).json({ username, accessToken, refreshToken });
        return;
      }
    }
  }

  async accessTokenController(req: Request, res: Response): Promise<void> {
    const refreshToken: string = req.body.refreshToken;
    const accessToken = await getAccessToken(refreshToken);

    if (!accessToken) {
      res.status(403).send("Invalid refresh token");
      return;
    }

    res
      .status(200)
      .cookie("refreshToken", refreshToken, {
        httpOnly: true,
        expires: new Date(Date.now() + 9000000),
      })
      .json({ accessToken, refreshToken });
  }
}

export function getRefreshToken(username: string): string {
  return jwt.sign({ username }, process.env.REFRESH_TOKEN_SECRET as string, {
    expiresIn: "30d",
  });
}

export function getAccessToken(refreshToken: string): Promise<string | null> {
  return new Promise((resolve) => {
    jwt.verify(
      refreshToken,
      process.env.REFRESH_TOKEN_SECRET as string,
      (err, decode) => {
        if (err || !decode) {
          return resolve(null);
        }

        const user = decode as DecodedPayload;
        const accessToken = jwt.sign(
          { username: user.username },
          process.env.ACCESS_TOKEN_SECRET as string,
          { expiresIn: 60 }
        );
        resolve(accessToken);
      }
    );
  });
}
