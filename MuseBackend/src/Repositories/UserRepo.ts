import { connect, model, Schema, Types } from "mongoose";
import type { Model } from "mongoose";

export interface User {
  _id?: Types.ObjectId;
  username: string;
  password: string;
}

export interface IUserRepo {
  createUser(user: User): Promise<User | null>;
  saveUser(user: User): Promise<User | null>;
  findById(id: string): Promise<User | null>;
  findByUsername(name: string): Promise<User | null>;
}

export class MongoUserRepo implements IUserRepo {
  private _schema: Schema;
  private _Model: Model<User>;
  private _connStr = process.env.MONGO_URL as string;

  constructor() {
    this._schema = new Schema<User>({
      username: { type: String, required: true },
      password: { type: String, required: true },
    });
    this._Model = model<User>("User", this._schema);
    this.connectToDb();
  }

  private async connectToDb() {
    try {
      await connect(this._connStr);
      console.log("Successfully connected to the MongoDB Database");
    } catch (e) {
      console.error(e, "Couldnt connect to the MongoDB Database");
    }
  }

  public async createUser(user: User): Promise<User | null> {
    const newUser = new this._Model(user);
    try {
      const result = await newUser.save();
      console.log("New user created successfully");
      return result;
    } catch (e) {
      console.error(e, "User couldn't be created");
      return null;
    }
  }

  public async saveUser(user: User): Promise<User | null> {
    return null;
  }

  public async findById(id: string): Promise<User | null> {
    const user = await this._Model.findById(id).exec();
    return user;
  }

  public async findByUsername(username: string): Promise<User | null> {
    const user = await this._Model.findOne({ username }).exec();
    return user;
  }
}
