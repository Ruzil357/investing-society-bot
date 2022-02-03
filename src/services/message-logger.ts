import { injectable } from 'inversify'
import { Client, Message, MessageEmbed, PartialMessage, TextChannel } from 'discord.js'

@injectable()
export class MessageLogger {
  private logChannel: string
  constructor() {
    if (!process.env.LOG_CHANNEL) {
      throw "Log Channel not found in environment variables"
    }

    this.logChannel = process.env.LOG_CHANNEL
  }

  public async messageCreated(message: Message) {
    if (!message.content) return;

    const embed = MessageLogger
      .generateEmbed(message)
      .setDescription(`Message sent by ${message.author} in <#${message.channel.id}>`)
      .addField('Content', message.content)

    const channel = await message.guild?.channels.fetch(this.logChannel) as TextChannel
    await channel.send({
      embeds: [embed]
    })
  }

  public async messageEdited(oldMessage: Message | PartialMessage, newMessage: Message) {
    if (!newMessage.content) return;

    const embed = MessageLogger
      .generateEmbed(newMessage)
      .setDescription(`Message edited by ${newMessage.author} in <#${newMessage.channel.id}>`)
      .addField('New Content', newMessage.content)

    if (oldMessage instanceof Message) {
      embed
        .addField('Old Content', oldMessage.content)
    }

    const channel = await newMessage.guild?.channels.fetch(this.logChannel) as TextChannel
    await channel.send({
      embeds: [embed]
    })
  }

  public async messageDeleted(message: Message) {
    if (!message.content) return;

    const embed = MessageLogger
      .generateEmbed(message)
      .setDescription(`Message deleted by ${message.author} in <#${message.channel.id}>`)
      .addField('Content', message.content)

    const channel = await message.guild?.channels.fetch(this.logChannel) as TextChannel
    await channel.send({
      embeds: [embed]
    })
  }

  private static generateEmbed(message: Message): MessageEmbed {
    return new MessageEmbed()
      .setTitle(`${message.author.username}#${message.author.discriminator}`)
      .setFooter({text: `Id: ${message.id}`})
      .setTimestamp(Date.now())
  }
}
