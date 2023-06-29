"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var express_1 = require("@jest-mock/express");
var httpHandler = require("../src/class/httphandler");
describe('http signaling test in public mode', function () {
    var sessionId = "abcd1234";
    var sessionId2 = "abcd5678";
    var connectionId = "12345";
    var connectionId2 = "67890";
    var testsdp = "test sdp";
    var _a = (0, express_1.getMockRes)(), res = _a.res, next = _a.next, mockClear = _a.mockClear;
    var req = (0, express_1.getMockReq)({ header: function () { return sessionId; } });
    var req2 = (0, express_1.getMockReq)({ header: function () { return sessionId2; } });
    beforeAll(function () {
        httpHandler.reset("public");
    });
    beforeEach(function () {
        mockClear();
    });
    test('throw check has session', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            httpHandler.checkSessionId(req, res, next);
            expect(res.sendStatus).toBeCalledWith(404);
            expect(next).not.toBeCalled();
            return [2 /*return*/];
        });
    }); });
    test('create session', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.createSession(sessionId, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ sessionId: sessionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.createSession(sessionId2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ sessionId: sessionId2 });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.createConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId, polite: true });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create connection from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId2 };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.createConnection(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId2, polite: true });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connections: [{ connectionId: connectionId }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post offer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, sdp: testsdp };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.postOffer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get offer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getOffer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ offers: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get offer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getOffer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ offers: [{ connectionId: connectionId, sdp: testsdp, polite: false }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post answer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, sdp: testsdp };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.postAnswer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get answer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getAnswer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ answers: [{ connectionId: connectionId, sdp: testsdp }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get answer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getAnswer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ answers: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post candidate from sesson1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, candidate: "testcandidate", sdpMLineIndex: 0, sdpMid: 0 };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.postCandidate(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get candidate from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getCandidate(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ candidates: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get candidate from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getCandidate(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ candidates: [{ connectionId: connectionId, candidates: [{ candidate: "testcandidate", sdpMLineIndex: 0, sdpMid: 0 }] }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete connection from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.deleteConnection(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('no connection get from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connections: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.deleteConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var req;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    req = (0, express_1.getMockReq)({ header: function () { return sessionId; } });
                    return [4 /*yield*/, httpHandler.deleteSession(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var req2;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    req2 = (0, express_1.getMockReq)({ header: function () { return sessionId2; } });
                    return [4 /*yield*/, httpHandler.deleteSession(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
});
describe('http signaling test in private mode', function () {
    var sessionId = "abcd1234";
    var sessionId2 = "abcd5678";
    var connectionId = "12345";
    var testsdp = "test sdp";
    var _a = (0, express_1.getMockRes)(), res = _a.res, next = _a.next, mockClear = _a.mockClear;
    var req = (0, express_1.getMockReq)({ header: function () { return sessionId; } });
    var req2 = (0, express_1.getMockReq)({ header: function () { return sessionId2; } });
    beforeAll(function () {
        httpHandler.reset("private");
    });
    beforeEach(function () {
        mockClear();
    });
    test('throw check has session', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            httpHandler.checkSessionId(req, res, next);
            expect(res.sendStatus).toBeCalledWith(404);
            expect(next).not.toBeCalled();
            return [2 /*return*/];
        });
    }); });
    test('create session', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.createSession(sessionId, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ sessionId: sessionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.createSession(sessionId2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ sessionId: sessionId2 });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.createConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId, polite: false });
                    return [2 /*return*/];
            }
        });
    }); });
    test('create connection from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.createConnection(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId, polite: true });
                    return [2 /*return*/];
            }
        });
    }); });
    test('response status 400 if connecctionId does not set', function () { return __awaiter(void 0, void 0, void 0, function () {
        var req3;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    req3 = (0, express_1.getMockReq)({ header: function () { return sessionId; } });
                    return [4 /*yield*/, httpHandler.createConnection(req3, res)];
                case 1:
                    _a.sent();
                    expect(res.status).toBeCalledWith(400);
                    expect(res.send).toBeCalledWith({ error: new Error("connectionId is required") });
                    return [2 /*return*/];
            }
        });
    }); });
    test('response status 400 if aleady used connection', function () { return __awaiter(void 0, void 0, void 0, function () {
        var sessionId3, body, req3;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    sessionId3 = "session3";
                    return [4 /*yield*/, httpHandler.createSession(sessionId3, res)];
                case 1:
                    _a.sent();
                    body = { connectionId: connectionId };
                    req3 = (0, express_1.getMockReq)({ header: function () { return sessionId3; } });
                    req3.body = body;
                    return [4 /*yield*/, httpHandler.createConnection(req3, res)];
                case 2:
                    _a.sent();
                    expect(res.status).toBeCalledWith(400);
                    expect(res.send).toBeCalledWith({ error: new Error("".concat(connectionId, ": This connection id is already used.")) });
                    return [2 /*return*/];
            }
        });
    }); });
    test('not connection get from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connections: [{ connectionId: connectionId }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post offer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, sdp: testsdp };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.postOffer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get offer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getOffer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ offers: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get offer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getOffer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ offers: [{ connectionId: connectionId, sdp: testsdp, polite: true }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post answer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, sdp: testsdp };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.postAnswer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get answer from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getAnswer(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ answers: [{ connectionId: connectionId, sdp: testsdp }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get answer from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getAnswer(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ answers: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('post candidate from sesson1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId, candidate: "testcandidate", sdpMLineIndex: 0, sdpMid: 0 };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.postCandidate(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('get candidate from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getCandidate(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ candidates: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get candidate from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getCandidate(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ candidates: [{ connectionId: connectionId, candidates: [{ candidate: "testcandidate", sdpMLineIndex: 0, sdpMid: 0 }] }] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete connection from session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req2.body = body;
                    return [4 /*yield*/, httpHandler.deleteConnection(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('get connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, httpHandler.getConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connections: [] });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete connection from session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var body;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    body = { connectionId: connectionId };
                    req.body = body;
                    return [4 /*yield*/, httpHandler.deleteConnection(req, res)];
                case 1:
                    _a.sent();
                    expect(res.json).toBeCalledWith({ connectionId: connectionId });
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete session1', function () { return __awaiter(void 0, void 0, void 0, function () {
        var req;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    req = (0, express_1.getMockReq)({ header: function () { return sessionId; } });
                    return [4 /*yield*/, httpHandler.deleteSession(req, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
    test('delete session2', function () { return __awaiter(void 0, void 0, void 0, function () {
        var req2;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    req2 = (0, express_1.getMockReq)({ header: function () { return sessionId2; } });
                    return [4 /*yield*/, httpHandler.deleteSession(req2, res)];
                case 1:
                    _a.sent();
                    expect(res.sendStatus).toBeCalledWith(200);
                    return [2 /*return*/];
            }
        });
    }); });
});
