import express from "express";
import "dotenv/config";
import jwt from "jsonwebtoken";

const authRouter = express.Router();

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

authRouter.post("/num", async (req, res) => {
  const accessToken = req.get("Authorization")?.split(" ")[1];

  if (!accessToken) {
    res.sendStatus(400);
    return;
  }

  jwt.verify(
    accessToken,
    process.env.ACCESS_TOKEN_SECRET as string,
    (err, _) => {
      if (err) {
        res.sendStatus(401);
        return;
      }

      res.status(200).json(numbers);
    }
  );
});

authRouter.post("/auth/signup", async (req, res) => {
  const username: string = req.body.username;
  const password: string = req.body.password;

  console.log(req.cookies);

  if (users.has(username)) {
    res.sendStatus(409);
    return;
  }

  const user: User = { username, password };
  users.set(username, user);

  // give access and refresh tokens
  const refreshToken = getRefreshToken(username);
  const accessToken = (await getAccessToken(refreshToken)) as string;

  res.status(200).json({ username, accessToken, refreshToken });
});

authRouter.post("/auth/login", async (req, res) => {
  const username: string = req.body.username;
  const password: string = req.body.password;

  if (!users.has(username)) {
    res.sendStatus(404);
    return;
  }

  if (users.has(username)) {
    const user = users.get(username);
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
});

authRouter.post("/auth/access-token", async (req, res) => {
  const refreshToken: string = req.body.refreshToken;
  const accessToken = await getAccessToken(refreshToken);

  if (!accessToken) {
    res.status(403).send("Invalid refresh token");
    return;
  }

  res.status(200).json({ accessToken, refreshToken });
});

function getRefreshToken(username: string): string {
  return jwt.sign({ username }, process.env.REFRESH_TOKEN_SECRET as string, {
    expiresIn: "30d",
  });
}

function getAccessToken(refreshToken: string): Promise<string | null> {
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

export default authRouter;
