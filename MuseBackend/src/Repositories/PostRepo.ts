import { connect, model, Schema, Types } from "mongoose";
import type { Model } from "mongoose";

export interface Post {
  _id?: Types.ObjectId;
  userid: string;
  username: string;
  content: string;
  contentHtml: string;
}

export interface IPostRepo {
  createPost(post: Post): Promise<Post | null>;
  savePost(post: Post): Promise<Post | null>;
  findById(id: string): Promise<Post | null>;
  findByUsername(username: string): Promise<Post | null>;
  findAllByUserid(userid: string): Promise<Post[] | null>;
  deletePost(id: string): Promise<boolean>;
}

export class MongoPostRepo implements IPostRepo {
  private _schema: Schema;
  private _Model: Model<Post>;
  private _connStr = process.env.MONGO_URL as string;

  constructor() {
    this._schema = new Schema<Post>({
      userid: { type: String, required: true },
      username: { type: String, required: true },
      content: { type: String, required: true },
      contentHtml: { type: String, required: true },
    });
    this._Model = model<Post>("Post", this._schema);
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

  public async createPost(post: Post): Promise<Post | null> {
    const newPost = new this._Model(post);
    try {
      const result = await newPost.save();
      console.log("New post created successfully");
      return result;
    } catch (e) {
      console.error(e, "Post couldn't be created");
      return null;
    }
  }

  public async savePost(post: Post): Promise<Post | null> {
    return null;
  }

  public async findById(id: string): Promise<Post | null> {
    const post = await this._Model.findById(id).exec();
    return post;
  }

  public async findByUsername(username: string): Promise<Post | null> {
    const post = await this._Model.findOne({ username }).exec();
    return post;
  }

  public async findAllByUserid(userid: string): Promise<Post[] | null> {
    const posts = await this._Model.find({ userid }).exec();
    return posts;
  }

  public async deletePost(id: string): Promise<boolean> {
    var response = await this._Model.findByIdAndDelete(id).exec();
    if (!response) return false;
    return true;
  }
}
