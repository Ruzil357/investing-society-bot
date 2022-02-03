require('dotenv').config();
import mongoose from 'mongoose'
import container from "./inversify.config";
import {TYPES} from "./types";
import {Bot} from "./bot";

if (!process.env.MONGODB_URL) {
  process.exit(1)
}
mongoose.connect(process.env.MONGODB_URL)

let bot = container.get<Bot>(TYPES.Bot);
bot.listen().then(() => {
  console.log('Logged in!')
}).catch((error) => {
  console.log('Oh no! ', error)
});
