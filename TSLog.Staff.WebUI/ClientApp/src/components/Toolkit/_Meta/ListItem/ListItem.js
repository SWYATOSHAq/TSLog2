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
            <button type="button" className="btn btn-outline-primary"><i className="fa-solid fa-arrow-right-arrow-left"></i></button>
            <button type="button" className="btn btn-outline-primary"><i className="fa-solid fa-id-card-clip"></i></button>
            <button type="button" className="btn btn-outline-danger"><i className='fa fa-trash-o'/></button>
            </div>
    );
    };
};
