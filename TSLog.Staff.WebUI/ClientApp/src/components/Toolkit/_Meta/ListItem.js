import React, { Component } from 'react';

export class ListItem extends Component {
    static displayName = ListItem.name;

    render() {
        const liStyle = {
            color: this.props.important ? 'tomato' : 'black'
        };
        return <span style={liStyle}>{this.props.label}</span>;
    };
};
