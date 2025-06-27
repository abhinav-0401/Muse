export interface IDbService<T> {
  get(key: string): T | null;
  set(key: string, value: T): void;
  has(key: string): boolean;
}

export class InMemoryDbService<T> implements IDbService<T> {
  private things: Map<string, T>;

  constructor() {
    this.things = new Map();
  }

  get(key: string): T | null {
    return this.things.get(key) ?? null;
  }

  set(key: string, value: T): void {
    this.things.set(key, value);
  }

  has(key: string): boolean {
    return this.things.has(key);
  }
}
