import React, { Component } from 'react';

export class BlockLevelButton extends Component {
    static displayName = BlockLevelButton.name;

    render() {

        return <button type="button" class="btn btn-primary btn-lg btn-block">Добавить</button>;
    }
}