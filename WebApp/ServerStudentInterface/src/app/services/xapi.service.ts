import { Injectable } from '@angular/core'
import XAPI, { Statement } from '@xapi/xapi'
import { Versions } from '@xapi/xapi/dist/types/constants/Versions'
import { stripPrefix } from '../utils/utils'

@Injectable({
  providedIn: 'root'
})
export class XapiService {
  constructor () {}

  sendStatement (actor: string, verb: string, object: string) {
    const endpoint = '' // Learninglocker endpoint goes here
    const auth = '' // Auth info here
    if (endpoint && auth) {
      const xapi = new XAPI({
        endpoint: endpoint,
        auth: auth,
        version: '1.0.3' as Versions // Version can be changed if needed
      })
      const myStatement: Statement = {
        actor: {
          mbox: `mailto:${stripPrefix(actor).replace(' ', '.')}@example.com`,
          name: stripPrefix(actor),
          objectType: 'Agent'
        },
        verb: {
          id: `http://example.com/xapi/${verb.replace(' ', '_')}`,
          display: { 'en-US': verb }
        },
        object: {
          id: `http://example.com/${object.replace(' ', '_')}`,
          definition: {
            name: { 'en-US': object.replace(' ', '_') },
            description: { 'en-US': object.replace(' ', '_') }
          },
          objectType: 'Activity'
        }
      }
      // console.log('Statement', myStatement)
      xapi.sendStatement({
        statement: myStatement
      })
    }
  }
}
