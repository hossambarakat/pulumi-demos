import axios from 'axios';

const baseUrl = process.env.REACT_APP_BASE_URL || "http://localhost:7071/";

export const loadMembers = async () => {
var response = await axios.get(`${baseUrl}api/members`);
const data = response.data;
const grouped = data.reduce((x, i) => {
    var groupedItem = x.filter(zone => zone.timeZone === i.timeZone)[0];
    if (groupedItem) {
      groupedItem.people.push(i);
    }
    else {
      var item = {
        timeZone: i.timeZone,
        people: [i]
      };
      x.push(item);
    }
    return x;
  }, []);

return grouped;
}

export const addMember = async (member) => {
    await axios.post(`${baseUrl}api/members`, member);
}

export const deleteMember = async (id) => {
    await axios.delete(`${baseUrl}api/members/${id}`);
}