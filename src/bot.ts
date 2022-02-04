import {Client, Message, TextChannel, Collection, GuildMember, MessageEmbed, PartialMessage, Invite} from 'discord.js'
import {inject, injectable} from 'inversify'
import {TYPES} from './types'
import {MessageLogger} from './services/message-logger'
import * as fs from 'fs'
import {setTimeout as wait} from 'timers/promises'
import {default as InviteModel, IInvite} from './models/invite.model'

@injectable()
export class Bot {
    private client: Client
    private readonly token: string
    private logger: MessageLogger
    private commands: Collection<string, any>
    private invites: Collection<string, any>
    private prefix: string

    constructor(
        @inject(TYPES.Client) client: Client,
        @inject(TYPES.DiscordToken) token: string,
        @inject(TYPES.MessageLogger) logger: MessageLogger,
    ) {
        this.client = client
        this.token = token
        this.logger = logger
        this.prefix = '!'
        this.commands = new Collection()
        this.invites = new Collection()

        const commandFiles = fs.readdirSync(`${__dirname}/commands/.`).filter(file => file.endsWith('.js') || file.endsWith('.ts'))
        for (const file of commandFiles) {
            const {default: command} = require(`${__dirname}/commands/${file}`)
            this.commands.set(command.name, command)
        }
    }

    public listen(): Promise<string> {
        this.client.on('ready', async () => {
            await wait(1000)

            const guild = this.client.guilds.cache.find(x => x.id === process.env.SERVER_ID)
            if (!guild) return

            const invites = await guild.invites.fetch()

            for (const invite of invites) {
                if (invite[1].uses) {
                    continue
                }
                this.invites.set(invite[1].code, <number>invite[1].uses)
            }
        })

        this.client.on('inviteDelete', (invite) => {
            // Delete the Invite from Cache
            this.invites.delete(invite.code)
        })

        this.client.on('inviteCreate', (invite) => {
            this.invites.set(invite.code, invite.uses)
        })


        this.client.on('messageCreate', async (message: Message) => {
            if (message.author.bot || !(message.channel instanceof TextChannel)) {
                return
            }

            if (message.channel.guildId == process.env.SERVER_ID && process.env.LOG_CHANNEL != message.channelId) {
                this.logger.messageCreated(message)
            }

            if (!message.content.startsWith(this.prefix)) return;
            const args: string[] = message.content.slice(this.prefix.length).split(/ +/)
            const command = args?.shift()?.toLowerCase()
            if (command && this.commands.has(command)) {
                this.commands.get(command).execute(message, args, this.client)
            }
        })

        this.client.on('messageUpdate', async (oldMessage: Message | PartialMessage, newMessage: Message | PartialMessage) => {
            if (newMessage.author?.bot || process.env.LOG_CHANNEL == newMessage.channelId || !(newMessage.channel instanceof TextChannel) || !(newMessage instanceof Message)) {
                return
            }

            if (newMessage.guildId === process.env.SERVER_ID) {
                this.logger.messageEdited(oldMessage, newMessage)
            }
        })

        this.client.on('messageDelete', async (message: Message | PartialMessage) => {
            if (message.author?.bot || process.env.LOG_CHANNEL == message.channelId || !(message.channel instanceof TextChannel) || !(message instanceof Message)) {
                return
            }


            if (message.guildId == process.env.SERVER_ID) {
                this.logger.messageDeleted(message)
            }
        })

        this.client.on('guildMemberAdd', async (member: GuildMember) => {
            console.log('new member!')
            if (member.guild.id != process.env.SERVER_ID) return

            console.log('member joined the right guild')
            const newInvites = await member.guild.invites.fetch()
            if (!newInvites) return

            console.log('new invites found')

            const invite = newInvites.find(i => <number>i.uses > this.invites.get(i.code))
            if (!invite) return

            console.log(`${member} joined using: ${invite.code}`)
            try {
                // fetch from mongo db
                const dbInvite: any = await InviteModel.findOne({inviteUrl: `https://discord.gg/${invite.code}`})
                if (dbInvite) {
                    // has been used
                    await invite.delete("used")

                    await member.setNickname(dbInvite.name)
                }
            } catch {
            }

            const embed = new MessageEmbed()
                .setTitle('Welcome to the server')
                .setDescription(`Thanks for joining the club, ${member}! We hope you enjoy your time here.`)

            if (!process.env.JOIN_CHANNEL || !process.env.MEMBER_ROLE) {
                throw 'Join channel or role not found in environment'
            }

            member.roles.add(process.env.MEMBER_ROLE)

            const channel = await member.guild.channels.fetch(process.env.JOIN_CHANNEL) as TextChannel
            if (!channel) return

            await channel.send({
                embeds: [embed],
            })
        })

        return this.client.login(this.token)
    }
}
