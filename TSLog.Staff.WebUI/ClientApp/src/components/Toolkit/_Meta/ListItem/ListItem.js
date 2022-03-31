import React, { Component } from 'react';

export class ListItem extends Component {
    static displayName = ListItem.name;

    render() {
        const liStyle = {
            color: this.props.important ? 'tomato' : 'black'
        };
        return (
            <div>
            <span style={liStyle}>{this.props.label}</span>
            <button type="button" className="btn btn-outline-primary">Изменить <i className='fa fa-eclamation'/></button>
            <button type="button" className="btn btn-outline-danger">Удалить <i className='fa fa-trash-o'/></button>
            </div>
    );
    };
};
