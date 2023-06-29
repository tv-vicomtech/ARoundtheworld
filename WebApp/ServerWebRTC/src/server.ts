import * as express from 'express';
import * as bodyParser from 'body-parser';
import * as path from 'path';
import * as fs from 'fs';
import * as morgan from 'morgan';
import signaling from './signaling';
import { log, LogLevel } from './log';
import Options from './class/options';
import { reset as resetHandler }from './class/httphandler';


export const createServer = (config: Options): express.Application => {
  const app: express.Application = express();
  const cors = require("cors");

  var corsOptions = {    
    origin: true,
    optionsSuccessStatus: 200, // For legacy browser support
    methods: "GET, PUT, POST, OPTIONS"
  };
  
  
  resetHandler(config.mode);
  // logging http access
  if (config.logging != "none") {
    app.use(morgan(config.logging));
  }
  // const signal = require('./signaling');
  app.use(bodyParser.urlencoded({ extended: true }));
  app.use(bodyParser.json());
  app.use(cors(corsOptions));
  app.get('/config', (req, res) => res.json({ useWebSocket: config.websocket, startupMode: config.mode, logging: config.logging }));
  app.use('/signaling', signaling);
  app.use(express.static(path.join(__dirname, '../client/public')));
  
  app.get('/', (req, res) => {
    const indexPagePath: string = path.join(__dirname, '../client/public/index.html');
    fs.access(indexPagePath, (err) => {
      if (err) {
        log(LogLevel.warn, `Can't find file ' ${indexPagePath}`);
        res.status(404).send(`Can't find file ${indexPagePath}`);
      } else {
        res.sendStatus(200);
        res.setHeader("Access-Control-Allow-Origin", "*");
        res.setHeader("Access-Control-Allow-Methods","POST, GET, OPTIONS");      
        res.sendFile(indexPagePath);
      
      }
    });
  });
  return app;
};
