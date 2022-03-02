import mongoose, {Schema} from 'mongoose'

export interface IInvite extends mongoose.Document {
  email: string,
  name: string,
  inviteUrl: string,
  discordId: string
}

const inviteModel = new Schema({
  email: {
    type: Schema.Types.String,
    required: true,
  },
  name: {
    type: Schema.Types.String,
    required: true,
  },
  inviteUrl: {
    type: Schema.Types.String,
    required: true,
  },
  discordId: {
    type: Schema.Types.String,
    required: false,
  },
  createdAt: {
    type: Schema.Types.Number,
    required: true,
  },
});


export default mongoose.model<IInvite>('Invite', inviteModel)
