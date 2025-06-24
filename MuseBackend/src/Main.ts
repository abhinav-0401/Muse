import express from "express";
import cors from "cors";

import assignRoutes from "./Router/Router.js";

const app = express();

app.use(cors());
app.use(express.json());

const PORT_NUMBER = 4321;

assignRoutes(app);

app.listen(PORT_NUMBER, "localhost", () => {
  console.log("the server's up and running, savvy?");
});
