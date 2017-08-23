'use strict';

var _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; };

(function () {

    var commands = {
        CREATE: 0,
        READ: 1,
        READALL: 2,
        UPDATE: 3,
        DELETE: 4,
        AGGREGATE: 5,
        getName: function getName(code) {
            return Object.keys(commands).find(function (key) {
                return commands[key] == code;
            });
        }
    };

    var isTransactionMode = false;
    var transactionPool = [];
    var BASE_URL = 'api/keyvalue';
    var requestKeyInput = document.querySelector('.main__input.main__input_key');
    var requestValueInput = document.querySelector('.main__input.main__input_value');

    function writeResultAsNode(inner) {
        var resultArea = document.querySelector('.result');
        var newNode = document.createElement('div');
        newNode.className = 'result__element';
        newNode.innerHTML = inner;
        if (resultArea.firstChild !== null) resultArea.insertBefore(newNode, resultArea.firstChild);else resultArea.appendChild(newNode);
    }

    function checkResponse(response) {
        if (response.ok) return response;
        throw response.statusText;
    };

    function writeErrorResult(command, error) {
        writeResultAsNode(commands.getName(JSON.parse(command.body).type) + " " + error);
    }

    function writeSuccessResult(command, response) {
        writeResultAsNode(prettifyResponse(command, response));
    }

    function prettifyResponse(command, response) {
        var commandBody = JSON.parse(command.body);
        var readAllPrint = function readAllPrint(response) {
            if (response.result.length == 0) return "Keys not found";
            result += '<ul>';
            result += response.result.map(function (it) {
                return '<li>' + it + '</li>';
            }).join('');
            result += '</ul>';
            return result;
        };
        var divIt = function divIt(string, classString) {
            return '<div class="' + classString + '">' + string + '</div>';
        };
        var result = "";
        switch (commandBody.type) {
            case commands.CREATE:
                result += divIt('CREATED: key(' + commandBody.key + '):value(' + commandBody.value + ') ', 'result__content result_created');break;
            case commands.READ:
                result += divIt('READ KEY ' + commandBody.key + ': ' + response.result + ' ', 'result__content result_read');break;
            case commands.READALL:
                result += divIt('READ ALL: ', 'result__content result_readall') + divIt(readAllPrint(response), 'result__readall');break;
            case commands.UPDATE:
                result += divIt('UPDATED: key(' + commandBody.key + '):value(' + commandBody.value + ') ', 'result__content result_updated');break;
            case commands.DELETE:
                result += divIt('DELETED: ' + commandBody.key + ' ', 'result__content result_deleted');break;
        }
        return result;
    }

    function sendCommand(command) {
        var commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD') commandOptions = _extends({}, command, { body: null });
        fetch(encodeURI(BASE_URL + command.url), commandOptions).then(checkResponse).then(function (response) {
            return response.json();
        }).then(function (response) {
            return writeSuccessResult(command, response);
        }).catch(function (response) {
            return writeErrorResult(command, response);
        });
    }

    function sendTransaction(command, transaction) {
        var commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD') commandOptions = _extends({}, command, { body: null });
        fetch(BASE_URL + command.url, commandOptions).then(checkResponse).then(function (response) {
            return response.json();
        }).then(function (response) {
            for (var i = 0; i < transaction.length; i++) {
                if (response[i].status === 0) writeSuccessResult(transaction[i], response[i]);else writeErrorResult(transaction[i], response[i].errorMessage);
            }
        }).catch(function (response) {
            return transaction.forEach(function (action) {
                return writeErrorResult(action, response);
            });
        });
    }

    var eventForClick = "click";

    function saveCommand(command) {
        transactionPool.push(command);
    }
    function createAction(e) {
        var command = {
            url: '/',
            method: 'POST',
            body: JSON.stringify({
                type: commands.CREATE,
                key: requestKeyInput.value,
                value: requestValueInput.value
            }),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (!isTransactionMode) sendCommand(command);else saveCommand(command);
    }
    document.querySelector('.actions__create').addEventListener(eventForClick, createAction);

    function getAction(e) {
        var command = {
            url: '/' + encodeURIComponent(requestKeyInput.value),
            method: 'GET',
            body: JSON.stringify({
                type: commands.READ,
                key: requestKeyInput.value
            }),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (requestKeyInput.value === '') return writeErrorResult(command, "No key provided");
        if (!isTransactionMode) sendCommand(command);else saveCommand(command);
    }
    document.querySelector('.actions__get').addEventListener(eventForClick, getAction);
    function getAllAction(e) {
        var command = {
            url: '/',
            method: 'GET',
            body: JSON.stringify({
                type: commands.READALL,
                key: requestKeyInput.value
            }),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (!isTransactionMode) sendCommand(command);else saveCommand(command);
    }
    document.querySelector('.actions__getall').addEventListener(eventForClick, getAllAction);

    function updateAction(e) {
        var command = {
            url: '/' + encodeURIComponent(requestKeyInput.value),
            method: 'PUT',
            body: JSON.stringify({
                type: commands.UPDATE,
                key: requestKeyInput.value,
                value: requestValueInput.value
            }),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (!isTransactionMode) sendCommand(command);else saveCommand(command);
    }
    document.querySelector('.actions__update').addEventListener(eventForClick, updateAction);

    function deleteAction(e) {
        var command = {
            url: '/' + encodeURIComponent(requestKeyInput.value),
            method: 'DELETE',
            body: JSON.stringify({
                type: commands.DELETE,
                key: requestKeyInput.value
            }),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (!isTransactionMode) sendCommand(command);else saveCommand(command);
    }
    document.querySelector('.actions__delete').addEventListener(eventForClick, deleteAction);

    function transactionAction(e) {
        var command = {
            url: '-actions',
            method: 'POST',
            body: JSON.stringify(transactionPool.map(function (action) {
                return JSON.parse(action.body);
            })),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (isTransactionMode && transactionPool.length > 0) sendTransaction(command, transactionPool);

        transactionPool = [];
        isTransactionMode = !isTransactionMode;
        e.target.classList.toggle('actions__transaction_active');
        if (isTransactionMode) e.target.textContent = 'End transaction';else e.target.textContent = 'Start transaction';
    }
    document.querySelector('.actions__transaction').addEventListener(eventForClick, transactionAction);
})();