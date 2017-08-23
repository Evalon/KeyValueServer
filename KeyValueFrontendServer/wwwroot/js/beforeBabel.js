'use strict';

(function() {

    var commands = {
        CREATE: 0,
        READ: 1,
        READALL: 2,
        UPDATE: 3,
        DELETE: 4,
        AGGREGATE: 5
    };

    var isTransactionMode = false;
    var transactionPool = [];
    var BASE_URL = 'api/keyvalue';
    var requestKeyInput = document.querySelector('.main__input.main__input_key');
    var requestValueInput = document.querySelector('.main__input.main__input_value');

    function writeResultAsNode(newNode) {
        var resultArea = document.querySelector('.result');
        if (!!resultArea.firstChild)
            resultArea.insertBefore(newNode, resultArea.firstChild);
        else
            resultArea.appendChild(newNode);
    }

    function checkResponse(response) {
        if (response.ok) return response;
        throw response.statusText;
    }

    function writeErrorResult(command, error) {
        writeResultAsNode(prettyNodeResponse(command, null, error));
    }

    function writeSuccessResult(command, response) {
        writeResultAsNode(prettyNodeResponse(command, response));
    }

    function prettyNodeResponse(command, response, error) {
        function getCommandNameNode(commandName, commandNameStyle) {
            let commandNameNode = document.createElement('span');
            commandNameNode.classList.add('result__command-name', `result__${commandNameStyle}`);
            commandNameNode.appendChild(document.createTextNode(commandName));
            return commandNameNode;
        }

        function getActionNode(commandName, keyValueList, commandNameStyle) {
            function getKeyValueNode(key, value) {
                let resultKeyValueNode = document.createElement('div');
                resultKeyValueNode.classList.add('key-value');

                function getKeyValueChild(text, name) {
                    let resultChildNode = document.createElement('span');
                    let resultChildTextNode = document.createTextNode(text);
                    resultChildNode.classList.add(`key-value__${name}`);
                    resultChildNode.appendChild(resultChildTextNode);
                    return resultChildNode;
                }
                if (!!key)
                    resultKeyValueNode.appendChild(getKeyValueChild(key, 'key'));
                if (value !== null)
                    resultKeyValueNode.appendChild(getKeyValueChild(value, 'value'));

                return resultKeyValueNode;
            }

            let resultActionNode = document.createElement('div');
            resultActionNode.classList.add('result__element');
            resultActionNode.appendChild(getCommandNameNode(commandName, commandNameStyle));

            if (!keyValueList) {
                resultActionNode.appendChild(document.createTextNode('List is empty'));
            } else if (keyValueList.length == 1) {
                resultActionNode.appendChild(getKeyValueNode(keyValueList[0].key, keyValueList[0].value));
            } else {
                let nodeUlList = document.createElement('ul');
                nodeUlList.classList.add('result__list');
                keyValueList.forEach((kv) => {
                    let nodeListElement = document.createElement('li');
                    nodeListElement.classList.add('result__list-element');
                    nodeListElement.appendChild(getKeyValueNode(kv.key, kv.value))
                    nodeUlList.appendChild(nodeListElement);
                })
                resultActionNode.appendChild(nodeUlList);
            }

            return resultActionNode;
        }

        function getSimpleActionNode(commandName, key, value, commandNameStyle) {
            return getActionNode(commandName, [{
                key,
                value
            }], commandNameStyle)
        }
        function getError(commandName, errorMessage, commandNameStyle) {
            let resultErrorNode = document.createElement('div');
            resultErrorNode.classList.add('result__element', 'result__element_error');
            resultErrorNode.appendChild(getCommandNameNode(commandName, commandNameStyle));
            let errorDescNode = document.createElement('span');
            errorDescNode.appendChild(document.createTextNode(errorMessage));
            resultErrorNode.appendChild(errorDescNode);
            return resultErrorNode;
        }

        var commandBody = JSON.parse(command.body);



        switch (commandBody.type) {
            case commands.CREATE:
                return !error ? getSimpleActionNode('Create', commandBody.key, commandBody.value, 'create') :
                    getError('Create', error, 'create');
            case commands.READ:
                return !error ? getSimpleActionNode('Read key', commandBody.key, response.result, 'read') :
                    getError('Read key', error, 'read');
            case commands.READALL:
                return !error ? getActionNode('Read all', response.result.map((key) => ({key, value: null})), 'read-all') :
                    getError('Read all', error, 'read-all');
            case commands.UPDATE:
                return !error ? getSimpleActionNode('Update', commandBody.key, commandBody.value, 'update') :
                    getError('Update', error, 'update');
            case commands.DELETE:
                return !error ? getSimpleActionNode('Delete', commandBody.key, null, 'update') :
                    getError('Delete', error, 'delete');
        }
    };



    function sendCommand(command) {
        var commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD') 
            commandOptions = {...command,  body: null };
        fetch(encodeURI(BASE_URL + command.url), commandOptions)
            .then(response => response.json().then(json => 
                {
                    if(response.ok)
                        return json;
                    throw { fromJson: json.errorMessage }
                }).catch((jsonText) => {throw jsonText.fromJson || response.statusText}))
            .then(response => writeSuccessResult(command, response))
            .catch(response => writeErrorResult(command, response));
    }

    function sendTransaction(command, transaction) {
        var commandOptions = command;
        if (command.method === 'GET' || command.method == 'HEAD') 
            commandOptions = {...command, body: null };
        fetch(BASE_URL + command.url, commandOptions)
        .then(checkResponse)
        .then(response => response.json())
        .then(response => {
            for (var i = 0; i < transaction.length; i++) {
                if (response[i].status === 0) writeSuccessResult(transaction[i], response[i]);
                else writeErrorResult(transaction[i], response[i].errorMessage);
            }
        }).catch(response => {
            return transaction.forEach(function(action) {
                return writeErrorResult(action, response);
            });
        });
    }

    var eventForClick = "click";
    document.querySelector('.actions__create').addEventListener(eventForClick, createAction);
    document.querySelector('.actions__get').addEventListener(eventForClick, getAction);
    document.querySelector('.actions__getall').addEventListener(eventForClick, getAllAction);
    document.querySelector('.actions__update').addEventListener(eventForClick, updateAction);
    document.querySelector('.actions__delete').addEventListener(eventForClick, deleteAction);

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
        if (!isTransactionMode) sendCommand(command);
        else saveCommand(command);
    }


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
        if (!isTransactionMode) sendCommand(command);
        else saveCommand(command);
    }

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
        if (!isTransactionMode) sendCommand(command);
        else saveCommand(command);
    }

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
        if (!isTransactionMode) sendCommand(command);
        else saveCommand(command);
    }
    
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
        if (!isTransactionMode) sendCommand(command);
        else saveCommand(command);
    }

    function transactionAction(e) {
        var command = {
            url: '-actions',
            method: 'POST',
            body: JSON.stringify(transactionPool.map(action => JSON.parse(action.body))),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        };
        if (isTransactionMode && transactionPool.length > 0) sendTransaction(command, transactionPool);

        transactionPool = [];
        isTransactionMode = !isTransactionMode;
        e.target.classList.toggle('actions__transaction_active');
        if (isTransactionMode) e.target.textContent = 'End transaction';
        else e.target.textContent = 'Start transaction';
    }
    document.querySelector('.actions__transaction').addEventListener(eventForClick, transactionAction);
})();