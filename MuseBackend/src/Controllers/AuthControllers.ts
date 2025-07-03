import type { Request, Response } from "express";
import "dotenv/config";
import jwt from "jsonwebtoken";
import { IUserRepo, User } from "../Repositories/UserRepo.js";

interface DecodedPayload {
  username: string;
}

export default class AuthController {
  constructor(private _userRepo: IUserRepo) {}

  async signupController(req: Request, res: Response): Promise<void> {
    const username: string = req.body.username;
    const password: string = req.body.password;

    console.log(req.cookies);

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
    const refreshToken = getRefreshToken(username);
    const accessToken = (await getAccessToken(refreshToken)) as string;

    res
      .status(200)
      .json({ username, accessToken, refreshToken, id: result._id });
  }

  async loginController(req: Request, res: Response): Promise<void> {
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
      const refreshToken = getRefreshToken(username);
      const accessToken = await getAccessToken(refreshToken);

      res
        .status(200)
        .json({ id: user._id, username, accessToken, refreshToken });
      return;
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
