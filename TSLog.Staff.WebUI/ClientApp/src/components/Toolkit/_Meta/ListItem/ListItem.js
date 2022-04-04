import React, { Component } from 'react';
import './ListItem.css'

export class ListItem extends Component {
    static di

    onLabelClick = () => {
        console.log(`Done: ${this.props.label}`);
    }
    // constructor() {
    //     super();
    //     this.onLabelClick = () => {
    //         console.log(`Done: ${this.props.label}`);
    //     }
    // }

    render() {
        const liStyle = {
            color: this.props.important ? 'tomato' : 'black'
        };
        return (
            <div className='change-button'>
                <span className='profession' style={liStyle} onClick={ this.onLabelClick } >{this.props.label}</span>
                <div>
                    <button type="button" className="btn btn-outline-primary button-icons"><i className="fa-solid fa-id-card-clip"></i></button>
                    <button type="button" className="btn btn-outline-danger button-icons" ><i className='fa fa-trash-o'/></button>
                </div>
            </div>
    );
    };
};
