import express from "express";
import cors from "cors";
import cookieParser from "cookie-parser";

import assignRoutes from "./Router/Router.js";

const app = express();

app.use(cors());
app.use(express.json());
app.use(cookieParser());

const PORT_NUMBER = 4321;

assignRoutes(app);

app.listen(PORT_NUMBER, () => {
  console.log("the server's up and running, savvy?");
});
