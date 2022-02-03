import "reflect-metadata";
import {Container} from "inversify";
import {TYPES} from "./types";
import {Bot} from "./bot";
import { Client, Intents } from 'discord.js'
import { MessageLogger } from './services/message-logger'
import * as Mongoose from 'mongoose'

let container = new Container();

container.bind<Bot>(TYPES.Bot).to(Bot).inSingletonScope();
container.bind<MessageLogger>(TYPES.MessageLogger).to(MessageLogger).inSingletonScope()
container.bind<Client>(TYPES.Client).toConstantValue(new Client({
  intents: [
    Intents.FLAGS.GUILD_MEMBERS,
    Intents.FLAGS.GUILDS,
    Intents.FLAGS.GUILD_MESSAGES,
    Intents.FLAGS.GUILD_INVITES,
  ],
}));

if (!process.env.DISCORD_TOKEN) {
  console.error("No token provided")
  process.exit(1)
}

container.bind<string>(TYPES.DiscordToken).toConstantValue(process.env.DISCORD_TOKEN);

export default container;
