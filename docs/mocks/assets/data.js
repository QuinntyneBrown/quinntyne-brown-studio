/* Fictional demo records. No credentials, uploads or personal data are sent anywhere. */
window.MockSeed = {
  rates: {wedding:350,events:225,headshots:180,family:200,distance:0.72,parking:20,meal:25,assistant:45,equipment:65},
  prints:[{id:'p1',name:'5 × 7 in',price:18,available:true},{id:'p2',name:'8 × 10 in',price:32,available:true},{id:'p3',name:'11 × 14 in',price:58,available:true},{id:'p4',name:'16 × 20 in',price:95,available:true}],
  sessions:[{id:'s1',name:'A day to remember',client:'Emma & Oliver',email:'emma@example.test',type:'Wedding',date:'2026-10-17',time:'14:00',duration:6,location:'Toronto Botanical Garden',photographer:'Quinntyne Brown',status:'Ready for review'},{id:'s2',name:'The little things',client:'The Bennett family',email:'alex@example.test',type:'Family',date:'2026-10-24',time:'10:00',duration:2,location:'High Park',photographer:'Maya Chen',status:'Scheduled'},{id:'s3',name:'A fresh perspective',client:'Noah Williams',email:'noah@example.test',type:'Headshots',date:'2026-10-28',time:'11:00',duration:1,location:'Daylight Studio',photographer:'Quinntyne Brown',status:'Delivered'}],
  photographers:[{id:'ph1',name:'Quinntyne Brown',email:'quinn@example.test',hours:'Monday–Saturday · 09:00–18:00',status:'Active'},{id:'ph2',name:'Maya Chen',email:'maya@example.test',hours:'Tuesday–Saturday · 09:00–18:00',status:'Active'}],
  equipment:[{id:'e1',name:'Canon EOS R5',category:'Camera',quantity:2,rate:65,status:'Available'},{id:'e2',name:'Profoto B10 lighting kit',category:'Lighting',quantity:1,rate:85,status:'Available'},{id:'e3',name:'85mm portrait lens',category:'Lens',quantity:2,rate:35,status:'In use'}],
  studios:[{id:'st1',name:'Daylight Studio',address:'120 Sample Street, Toronto',area:'Toronto',rate:95,status:'Available'},{id:'st2',name:'The White Room',address:'48 Example Avenue, Hamilton',area:'Hamilton',rate:75,status:'Available'}],
  discounts:[{id:'d1',name:'Plan ahead',type:'Advance booking',percent:10,days:90,code:'',weekday:'Tuesday',active:true},{id:'d2',name:'A quieter Tuesday',type:'Slow day',percent:8,days:0,code:'',weekday:'Tuesday',active:true},{id:'d3',name:'A little welcome',type:'Promo code',percent:12,days:0,code:'HELLO12',weekday:'Tuesday',active:true}],
  galleries:[{id:'g1',name:'Together, naturally',category:'Weddings',description:'The big feelings. The little moments. All of it, yours.',status:'Published',photos:[0,1,2,3]},{id:'g2',name:'Ordinary magic',category:'Family',description:'Room to play. Space to be yourselves.',status:'Published',photos:[4,5,6]},{id:'g3',name:'In good company',category:'Events',description:'People coming together, beautifully.',status:'Draft',photos:[2,6,7]}],
  content:[{id:'home',name:'Home',heading:'For the moments\nthat become everything.',body:'Honest photographs. Thoughtfully made. A Toronto photography studio for the people and moments that matter.',status:'Published'},{id:'services',name:'Services',heading:'Your story. Your way.',body:'From a room full of people to a moment just for you. Photography that feels like you.',status:'Published'},{id:'contact',name:'Contact',heading:'Something beautiful\nstarts with hello.',body:'Tell us what you have in mind. We’ll find the right way to capture it.',status:'Published'}],
  promotions:[{id:'pr1',name:'The wedding collection',description:'Six hours of storytelling, a thoughtfully edited gallery, and a planning consultation.',price:1950,status:'Published'},{id:'pr2',name:'A little family time',description:'An unhurried hour outdoors and a gallery full of personality.',price:240,status:'Published'},{id:'pr3',name:'Your next chapter',description:'A relaxed headshot session, two looks, and your five favourite portraits.',price:320,status:'Published'}],
  vendors:[{id:'v1',name:'Leah Morgan',specialty:'Makeup artist',email:'leah@example.test',phone:'416-555-0131',rate:120},{id:'v2',name:'Sam Rivera',specialty:'Second shooter',email:'sam@example.test',phone:'416-555-0142',rate:85},{id:'v3',name:'Jamie Park',specialty:'Assistant',email:'jamie@example.test',phone:'416-555-0183',rate:45}],
  blockedDates:['2026-10-18'],
  schedules:[{id:'sc1',name:'Personal day',date:'2026-10-18',photographer:'All photographers',time:'09:00',end:'18:00',kind:'Unavailable'}],
  albums:[{id:'a1',name:'Our favourites',description:'The ones we keep coming back to.',photos:[0,1,3,5]},{id:'a2',name:'For the family',description:'A little collection to share.',photos:[2,4,6]}],
  requests:[{id:'QB-1042',date:'2026-08-28',status:'With the studio',name:'Emma Harper',email:'emma@example.test',notes:'Matte paper, please.',items:[{photo:0,size:'p2',qty:2,price:32},{photo:3,size:'p3',qty:1,price:58}],total:122}],
  cart:[{photo:0,size:'p2',qty:1},{photo:3,size:'p3',qty:1}],
  reviewSelected:[0,3],
  quote:null
};
window.MockPhotos = [
  {file:'wedding.jpg',alt:'A couple celebrating their wedding outdoors',label:'The beginning'},
  {file:'couple.jpg',alt:'A garden wedding gazebo decorated with flowers and white chairs',label:'Just us'},
  {file:'flowers.jpg',alt:'Pastel flowers arranged along a wedding reception table',label:'Small details'},
  {file:'ceremony.jpg',alt:'A newly married couple embracing beside the water',label:'All the feeling'},
  {file:'family.jpg',alt:'A family spending time together outdoors',label:'Ordinary magic'},
  {file:'portrait.jpg',alt:'A portrait in soft natural light',label:'A little pause'},
  {file:'gathering.jpg',alt:'A celebration table set with colourful flowers and glassware',label:'In good company'},
  {file:'landscape.jpg',alt:'A quiet landscape in warm light',label:'Room to breathe'}
];
