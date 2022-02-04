import { Card } from '../_Meta/Card';

export class ShiftCard extends Card {
    static displayName = ShiftCard.name;

    constructor(props) {
        super(props);
    }

    render() {
        let card = super.render();

        console.log(super.state);

        return (
            <div>
                <p>ShiftCard</p>
                {card}

            </div>
        );
    }
}
