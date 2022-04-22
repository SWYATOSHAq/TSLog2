export default class LoadInfo {
    async getResource(url) {
        const res = await fetch(url);
        if (!res.ok) {
            throw new Error(`Could not fetch ${url}` +
                `, received ${res.status}`)
        }
        return await res.json();
    }

    async getData() {
        const res = await this.getResource('/api/v1/Staff/Position');
     return res
    }

    async getProfessions() {
        const professions = await this.getData().then(body => body.map((element) => element.DisplayName));
        return await professions
    }

    async getId() {
        const id = await this.getData().then(body => body.map((element) => element.Id));
        return await id
    }

}



// const professions = new LoadInfo();
// professions.getProfessions().then((body) => {
//     console.log(body);
// });

