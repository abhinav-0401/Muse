import type { Request, Response } from "express";
import "dotenv/config";
import jwt from "jsonwebtoken";
import { IUserRepo, User } from "../Repositories/UserRepo.js";

interface DecodedPayload {
  username: string;
  _id: string;
}

export default class AuthController {
  constructor(private _userRepo: IUserRepo) {}

  public async signupController(req: Request, res: Response): Promise<void> {
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
    const refreshToken = getRefreshToken(user);
    const accessToken = (await getAccessToken(refreshToken)) as string;

    res
      .status(200)
      .json({ username, accessToken, refreshToken, id: result._id });
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

      res
        .status(200)
        .json({ id: user._id, username, accessToken, refreshToken });
      return;
    }
  }

  public async userInfoController(req: Request, res: Response): Promise<void> {
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

    const user = this._userRepo.findById(decoded._id);
    if (!user) {
      res.send("couldn't find user");
      return;
    }

    res.status(200).json(user);
  }

  public async accessTokenController(
    req: Request,
    res: Response
  ): Promise<void> {
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

  private verifyAccessToken(token: string): Promise<DecodedPayload | null> {
    return new Promise((resolve) => {
      jwt.verify(
        token,
        process.env.ACCESS_TOKEN_SECRET as string,
        (err, decode) => {
          if (err || !decode) {
            return resolve(null);
          }

          return decode as DecodedPayload;
        }
      );
    });
  }
}

export function getRefreshToken(user: User): string {
  return jwt.sign(
    { username: user.username, id: user._id },
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
          return resolve(null);
        }

        const user = decode as DecodedPayload;
        const accessToken = jwt.sign(
          { username: user.username, id: user._id },
          process.env.ACCESS_TOKEN_SECRET as string,
          { expiresIn: 60 }
        );
        resolve(accessToken);
      }
    );
  });
}
