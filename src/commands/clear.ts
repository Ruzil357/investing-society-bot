import { Client, Message } from 'discord.js'

const execute = async (message: Message, args: any[], client: Client) => {
  if (!args[0] || typeof +args[0] !== 'number') return

  const permissions = message.member?.permissionsIn(message.channelId)
  if (permissions && !permissions.has('ADMINISTRATOR') && !permissions.has('MANAGE_MESSAGES')) {
    return
  }

  const items = await message.channel.messages.fetch({ limit: +args[0] + 1 })

  await items.forEach(async message => await message.delete())
}
export default {
  name: "clear",
  execute
}
