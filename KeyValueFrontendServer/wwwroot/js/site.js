(function () {

    commands = {
        CREATE: 0,
        READ: 1,
        READALL: 2,
        UPDATE: 3,
        DELETE: 4,
        AGGREGATE: 5,
        getName: (code) => {
            return Object.keys(commands).find(key => commands[key] == code);
        }
    };

    let isTransactionMode = false;
    let transactionPool = [];
    const BASE_URL = 'api/keyvalue';
    let requestKeyInput = document.querySelector('.main__input.main__input_key');
    let requestValueInput = document.querySelector('.main__input.main__input_value');

    function writeResultAsNode(inner) {
        let resultArea = document.querySelector('.result');
        let newNode = document.createElement('div');
        newNode.className = 'result__element';
        newNode.innerHTML = inner;
        if (resultArea.firstChild !== null)
            resultArea.insertBefore(newNode, resultArea.firstChild);
        else
            resultArea.appendChild(newNode)
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
        let commandBody = JSON.parse(command.body);
        let readAllPrint = (response) => {
            if(response.result.length == 0) return "Keys not found";
            result += '<ul>';
            result += response.result.map((it) => '<li>' + it + '</li>').join('');
            result += '</ul>';
            return result;
        };
        let divIt = (string, classString) => `<div class="${classString}">${string}</div>`;
        let result = "";
        switch (commandBody.type) {
            case (commands.CREATE):
                result += divIt(`CREATED: key(${commandBody.key}):value(${commandBody.value}) `, 'result__content result_created'); break;
            case (commands.READ):
                result += divIt(`READ KEY ${commandBody.key}: ${response.result} `, 'result__content result_read'); break;
            case (commands.READALL):
                result += divIt('READ ALL: ', 'result__content result_readall') + divIt(readAllPrint(response), 'result__readall'); break;
            case (commands.UPDATE):
                result += divIt(`UPDATED: key(${commandBody.key}):value(${commandBody.value}) `, 'result__content result_updated'); break;
            case (commands.DELETE):
                result += divIt(`DELETED: ${commandBody.key} `, 'result__content result_deleted'); break;
        }
        return result;
    }

    function sendCommand(command) {
        let commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD')
            commandOptions = {...command, body: null };
        fetch(encodeURI(BASE_URL + command.url), commandOptions)
            .then(checkResponse)
            .then(response => response.json())
            .then(response => writeSuccessResult(command, response))
            .catch(response => writeErrorResult(command, response));
    }

    function sendTransaction(command, transaction) {
        let commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD')
            commandOptions = { ...command, body: null };
        fetch(BASE_URL + command.url, commandOptions)
            .then(checkResponse)
            .then(response => response.json())
            .then(response => {
                for (let i = 0; i < transaction.length; i++) {
                    if (response[i].status === 0)
                        writeSuccessResult(transaction[i], response[i])
                    else
                        writeErrorResult(transaction[i], response[i].errorMessage);
                }
            })
            .catch((response) => transaction.forEach((action) => writeErrorResult(action, response))
                );
    }

    function saveCommand(command) {
        transactionPool.push(command);
    }

    document.querySelector('.actions__create').addEventListener('click', (e) => {
        let command = {
            url: encodeURI('/'),
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
        }
        if (!isTransactionMode)
            sendCommand(command);
        else
            saveCommand(command);
    });

    document.querySelector('.actions__get').addEventListener('click', (e) => {
        let command = {
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
        }
	if (requestKeyInput.value === '') 
		return writeErrorResult(command, "No key provided");
        if (!isTransactionMode)
            sendCommand(command);
        else
            saveCommand(command);
    });

    document.querySelector('.actions__getall').addEventListener('click', (e) => {
        let command = {
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
        }
        if (!isTransactionMode)
            sendCommand(command);
        else
            saveCommand(command);
    });

    document.querySelector('.actions__update').addEventListener('click', (e) => {
        let command = {
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
        }
        if (!isTransactionMode)
            sendCommand(command);
        else
            saveCommand(command);
    });

    document.querySelector('.actions__delete').addEventListener('click', (e) => {
        let command = {
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
        }
        if (!isTransactionMode)
            sendCommand(command);
        else
            saveCommand(command);
    });

    document.querySelector('.actions__transaction').addEventListener('click', (e) => {
        let command = {
            url: '-actions',
            method: 'POST',
            body: JSON.stringify(transactionPool.map(action => JSON.parse(action.body))),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }
        if (isTransactionMode && transactionPool.length > 0)
            sendTransaction(command, transactionPool);

        transactionPool = [];
        isTransactionMode = !isTransactionMode;
        e.target.classList.toggle('actions__transaction_active');
        if (isTransactionMode)
            e.target.textContent = 'End Transaction';
        else
            e.target.textContent = 'Start Transaction';
    });
})()