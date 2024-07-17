-- CREATE DATABASE mobble
-- DB name mobble
create table mobble_dispatcher.stops_virtual
(
    vstop_idx      integer generated always as identity,
    vstop_name     varchar(100)          not null,
    vstop_loc      varchar(200),
    nearby_link_id integer,
    region_code    integer,
    stops_area_idx integer,
    input_date     varchar(50),
    address        varchar(500),
    geom           varchar(500)          not null,
    is_use         boolean default false not null
);

create table mobble_dispatcher.fare_base
(
    id                           integer generated always as identity,
    fare_once                    integer   default 3000  not null,
    fare_month                   integer   default 50000 not null,
    basic_distance               real      default 1.6   not null,
    basic_distance_fare          integer   default 3000  not null,
    basic_speed                  smallint  default 15    not null,
    basic_combined_time          smallint  default 30    not null,
    basic_combined_time_fare     smallint  default 100   not null,
    basic_combined_distance      smallint  default 131   not null,
    basic_combined_distance_fare smallint  default 100   not null,
    create_time                  timestamp default now() not null,
    update_time                  timestamp
);



create table mobble_dispatcher.tranportation_cost_base
(
    id                        integer generated always as identity,
    diesel_cost               smallint  default 1400   not null,
    electricity_cost          smallint  default 1200   not null,
    man_day_cost              integer   default 187000 not null,
    man_insurance             real      default 14.1   not null,
    vehicle_depreciation_rate smallint  default 10     not null,
    vehicle_insurance         integer   default 200000 not null,
    etc_cost                  integer   default 30000  not null,
    day_run_time              smallint  default 8      not null,
    line_price_distance       integer   default 30000  not null,
    line_price_time           integer   default 150000 not null,
    create_time               timestamp default now()  not null,
    update_time               timestamp
);


create table mobble_dispatcher.fuel_type
(
    id   integer generated always as identity
        primary key,
    type varchar(20)
);



create table mobble_dispatcher.carbon_footprint
(
    id                  integer generated always as identity
        primary key,
    fuel_type_id        integer     default 1                         not null
        constraint fk_fuel_type
            references mobble_dispatcher.fuel_type,
    seats_size          smallint,
    cf_value            smallint,
    fuel_efficiency     smallint,
    vehicle_price       integer,
    cf_unit             varchar(20) default 'g/km'::character varying not null,
    fe_diesel_unit      varchar(20) default 'km/l'::character varying not null,
    fe_electricity_unit varchar(20) default 'km/w'::character varying not null,
    create_time         timestamp   default now()                     not null,
    update_time         timestamp
);


-- 실제 사용을 위해서 기본 데이터 추가
insert into mobble_dispatcher.carbon_footprint (id, fuel_type_id, seats_size, cf_value, fuel_efficiency, vehicle_price, cf_unit, fe_diesel_unit, fe_electricity_unit, create_time, update_time)
values  (2, 1, 45, 34, 4, 250000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:53:00.838229', null),
        (3, 1, 25, 22, 6, 180000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:53:14.303118', null),
        (4, 1, 16, 16, 7, 80000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:57:15.420613', null),
        (5, 2, 45, 0, 3, 250000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:58:20.811674', null),
        (6, 2, 25, 0, 4, 180000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:58:20.811674', null),
        (7, 2, 16, 0, 5, 80000000, 'g/km', 'km/l', 'km/w', '2024-04-14 03:58:20.811674', null);


insert into mobble_dispatcher.fare_base (id, fare_once, fare_month, basic_distance, basic_distance_fare, basic_speed, basic_combined_time, basic_combined_time_fare, basic_combined_distance, basic_combined_distance_fare, create_time, update_time)
values  (1, 3000, 50000, 1.6, 3000, 15, 30, 100, 131, 100, '2024-04-14 03:11:57.852123', null);

insert into mobble_dispatcher.fuel_type (id, type)
values  (1, 'diesel'),
        (2, 'electricity');

insert into mobble_dispatcher.stops_virtual (vstop_idx, vstop_name, vstop_loc, nearby_link_id, region_code, stops_area_idx, input_date, address, geom, is_use)
values  (1, '상평교차로', '(124,37)', null, 1, null, null, null, '010100002042140000F20F583942C30D4178C0CA81A6A12041', true);

insert into mobble_dispatcher.tranportation_cost_base (id, diesel_cost, electricity_cost, man_day_cost, man_insurance, vehicle_depreciation_rate, vehicle_insurance, etc_cost, day_run_time, line_price_distance, line_price_time, create_time, update_time)
values  (1, 1400, 1200, 187000, 14.1, 10, 200000, 30000, 8, 30000, 150000, '2024-04-14 03:11:44.987329', null);