﻿namespace Ninu.Emulator.CentralProcessor
{
    public enum NewOpcode : byte
    {
        Brk_Implied = 0x00,
        Ora_IndirectZeroPageWithXOffset = 0x01,
        Kil_Implied_02 = 0x02, // Illegal
        Slo_IndirectZeroPageWithXOffset_03 = 0x03, // Illegal
        Nop_ZeroPage_04 = 0x04, // Illegal
        Ora_ZeroPage = 0x05,
        Asl_ZeroPage = 0x06,
        Slo_ZeroPage_07 = 0x07, // Illegal
        Php_Implied = 0x08,
        Ora_Immediate = 0x09,
        Asl_Accumulator = 0x0A,
        Anc_Immediate_0B = 0x0B, // Illegal
        Nop_Absolute_0C = 0x0C, // Illegal
        Ora_Absolute = 0x0D,
        Asl_Absolute = 0x0E,
        Slo_Absolute_0F = 0x0F, // Illegal
        Bpl_Relative = 0x10,
        Ora_IndirectZeroPageWithYOffset = 0x11,
        Kil_Implied_12 = 0x12, // Illegal
        Slo_IndirectZeroPageWithYOffset_13 = 0x13, // Illegal
        Nop_ZeroPageWithXOffset_14 = 0x14, // Illegal
        Ora_ZeroPageWithXOffset = 0x15,
        Asl_ZeroPageWithXOffset = 0x16,
        Slo_ZeroPageWithXOffset_17 = 0x17, // Illegal
        Clc_Implied = 0x18,
        Ora_AbsoluteWithYOffset = 0x19,
        Nop_Implied_1A = 0x1A, // Illegal
        Slo_AbsoluteWithYOffset_1B = 0x1B, // Illegal
        Nop_AbsoluteWithXOffset_1C = 0x1C, // Illegal
        Ora_AbsoluteWithXOffset = 0x1D,
        Asl_AbsoluteWithXOffset = 0x1E,
        Slo_AbsoluteWithXOffset_1F = 0x1F, // Illegal
        Jsr_Absolute = 0x20,
        And_IndirectZeroPageWithXOffset = 0x21,
        Kil_Implied_22 = 0x22, // Illegal
        Rla_IndirectZeroPageWithXOffset_23 = 0x23, // Illegal
        Bit_ZeroPage = 0x24,
        And_ZeroPage = 0x25,
        Rol_ZeroPage = 0x26,
        Rla_ZeroPage_27 = 0x27, // Illegal
        Plp_Implied = 0x28,
        And_Immediate = 0x29,
        Rol_Accumulator = 0x2A,
        Anc_Immediate_2B = 0x2B, // Illegal
        Bit_Absolute = 0x2C,
        And_Absolute = 0x2D,
        Rol_Absolute = 0x2E,
        Rla_Absolute_2F = 0x2F, // Illegal
        Bmi_Relative = 0x30,
        And_IndirectZeroPageWithYOffset = 0x31,
        Kil_Implied_32 = 0x32, // Illegal
        Rla_IndirectZeroPageWithYOffset_33 = 0x33, // Illegal
        Nop_ZeroPageWithXOffset_34 = 0x34, // Illegal
        And_ZeroPageWithXOffset = 0x35,
        Rol_ZeroPageWithXOffset = 0x36,
        Rla_ZeroPageWithXOffset_37 = 0x37, // Illegal
        Sec_Implied = 0x38,
        And_AbsoluteWithYOffset = 0x39,
        Nop_Implied_3A = 0x3A, // Illegal
        Rla_AbsoluteWithYOffset_3B = 0x3B, // Illegal
        Nop_AbsoluteWithXOffset_3C = 0x3C, // Illegal
        And_AbsoluteWithXOffset = 0x3D,
        Rol_AbsoluteWithXOffset = 0x3E,
        Rla_AbsoluteWithXOffset_3F = 0x3F, // Illegal
        Rti_Implied = 0x40,
        Eor_IndirectZeroPageWithXOffset = 0x41,
        Kil_Implied_42 = 0x42, // Illegal
        Sre_IndirectZeroPageWithXOffset_43 = 0x43, // Illegal
        Nop_ZeroPage_44 = 0x44, // Illegal
        Eor_ZeroPage = 0x45,
        Lsr_ZeroPage = 0x46,
        Sre_ZeroPage_47 = 0x47, // Illegal
        Pha_Implied = 0x48,
        Eor_Immediate = 0x49,
        Lsr_Accumulator = 0x4A,
        Alr_Immediate_4B = 0x4B, // Illegal
        Jmp_Absolute = 0x4C,
        Eor_Absolute = 0x4D,
        Lsr_Absolute = 0x4E,
        Sre_Absolute_4F = 0x4F, // Illegal
        Bvc_Relative = 0x50,
        Eor_IndirectZeroPageWithYOffset = 0x51,
        Kil_Implied_52 = 0x52, // Illegal
        Sre_IndirectZeroPageWithYOffset_53 = 0x53, // Illegal
        Nop_ZeroPageWithXOffset_54 = 0x54, // Illegal
        Eor_ZeroPageWithXOffset = 0x55,
        Lsr_ZeroPageWithXOffset = 0x56,
        Sre_ZeroPageWithXOffset_57 = 0x57, // Illegal
        Cli_Implied = 0x58,
        Eor_AbsoluteWithYOffset = 0x59,
        Nop_Implied_5A = 0x5A, // Illegal
        Sre_AbsoluteWithYOffset_5B = 0x5B, // Illegal
        Nop_AbsoluteWithXOffset_5C = 0x5C, // Illegal
        Eor_AbsoluteWithXOffset = 0x5D,
        Lsr_AbsoluteWithXOffset = 0x5E,
        Sre_AbsoluteWithXOffset_5F = 0x5F, // Illegal
        Rts_Implied = 0x60,
        Adc_IndirectZeroPageWithXOffset = 0x61,
        Kil_Implied_62 = 0x62, // Illegal
        Rra_IndirectZeroPageWithXOffset_63 = 0x63, // Illegal
        Nop_ZeroPage_64 = 0x64, // Illegal
        Adc_ZeroPage = 0x65,
        Ror_ZeroPage = 0x66,
        Rra_ZeroPage_67 = 0x67, // Illegal
        Pla_Implied = 0x68,
        Adc_Immediate = 0x69,
        Ror_Accumulator = 0x6A,
        Arr_Immediate_6B = 0x6B, // Illegal
        Jmp_Indirect = 0x6C,
        Adc_Absolute = 0x6D,
        Ror_Absolute = 0x6E,
        Rra_Absolute_6F = 0x6F, // Illegal
        Bvs_Relative = 0x70,
        Adc_IndirectZeroPageWithYOffset = 0x71,
        Kil_Implied_72 = 0x72, // Illegal
        Rra_IndirectZeroPageWithYOffset_73 = 0x73, // Illegal
        Nop_ZeroPageWithXOffset_74 = 0x74, // Illegal
        Adc_ZeroPageWithXOffset = 0x75,
        Ror_ZeroPageWithXOffset = 0x76,
        Rra_ZeroPageWithXOffset_77 = 0x77, // Illegal
        Sei_Implied = 0x78,
        Adc_AbsoluteWithYOffset = 0x79,
        Nop_Implied_7A = 0x7A, // Illegal
        Rra_AbsoluteWithYOffset_7B = 0x7B, // Illegal
        Nop_AbsoluteWithXOffset_7C = 0x7C, // Illegal
        Adc_AbsoluteWithXOffset = 0x7D,
        Ror_AbsoluteWithXOffset = 0x7E,
        Rra_AbsoluteWithXOffset_7F = 0x7F, // Illegal
        Nop_Immediate_80 = 0x80, // Illegal
        Sta_IndirectZeroPageWithXOffset = 0x81,
        Nop_Immediate_82 = 0x82, // Illegal
        Sax_IndirectZeroPageWithXOffset_83 = 0x83, // Illegal
        Sty_ZeroPage = 0x84,
        Sta_ZeroPage = 0x85,
        Stx_ZeroPage = 0x86,
        Sax_ZeroPage_87 = 0x87, // Illegal
        Dey_Implied = 0x88,
        Nop_Immediate_89 = 0x89, // Illegal
        Txa_Implied = 0x8A,
        Xaa_Immediate_8B = 0x8B, // Illegal
        Sty_Absolute = 0x8C,
        Sta_Absolute = 0x8D,
        Stx_Absolute = 0x8E,
        Sax_Absolute_8F = 0x8F, // Illegal
        Bcc_Relative = 0x90,
        Sta_IndirectZeroPageWithYOffset = 0x91,
        Kil_Implied_92 = 0x92, // Illegal
        Ahx_IndirectZeroPageWithYOffset_93 = 0x93, // Illegal
        Sty_ZeroPageWithXOffset = 0x94,
        Sta_ZeroPageWithXOffset = 0x95,
        Stx_ZeroPageWithYOffset = 0x96,
        Sax_ZeroPageWithYOffset_97 = 0x97, // Illegal
        Tya_Implied = 0x98,
        Sta_AbsoluteWithYOffset = 0x99,
        Txs_Implied = 0x9A,
        Tas_AbsoluteWithYOffset_9B = 0x9B, // Illegal
        Shy_AbsoluteWithXOffset_9C = 0x9C, // Illegal
        Sta_AbsoluteWithXOffset = 0x9D,
        Shx_AbsoluteWithYOffset_9E = 0x9E, // Illegal
        Ahx_AbsoluteWithYOffset_9F = 0x9F, // Illegal
        Ldy_Immediate = 0xA0,
        Lda_IndirectZeroPageWithXOffset = 0xA1,
        Ldx_Immediate = 0xA2,
        Lax_IndirectZeroPageWithXOffset_A3 = 0xA3, // Illegal
        Ldy_ZeroPage = 0xA4,
        Lda_ZeroPage = 0xA5,
        Ldx_ZeroPage = 0xA6,
        Lax_ZeroPage_A7 = 0xA7, // Illegal
        Tay_Implied = 0xA8,
        Lda_Immediate = 0xA9,
        Tax_Implied = 0xAA,
        Lax_Immediate_AB = 0xAB, // Illegal
        Ldy_Absolute = 0xAC,
        Lda_Absolute = 0xAD,
        Ldx_Absolute = 0xAE,
        Lax_Absolute_AF = 0xAF, // Illegal
        Bcs_Relative = 0xB0,
        Lda_IndirectZeroPageWithYOffset = 0xB1,
        Kil_Implied_B2 = 0xB2, // Illegal
        Lax_IndirectZeroPageWithYOffset_B3 = 0xB3, // Illegal
        Ldy_ZeroPageWithXOffset = 0xB4,
        Lda_ZeroPageWithXOffset = 0xB5,
        Ldx_ZeroPageWithYOffset = 0xB6,
        Lax_ZeroPageWithYOffset_B7 = 0xB7, // Illegal
        Clv_Implied = 0xB8,
        Lda_AbsoluteWithYOffset = 0xB9,
        Tsx_Implied = 0xBA,
        Las_AbsoluteWithYOffset_BB = 0xBB, // Illegal
        Ldy_AbsoluteWithXOffset = 0xBC,
        Lda_AbsoluteWithXOffset = 0xBD,
        Ldx_AbsoluteWithYOffset = 0xBE,
        Lax_AbsoluteWithYOffset_BF = 0xBF, // Illegal
        Cpy_Immediate = 0xC0,
        Cmp_IndirectZeroPageWithXOffset = 0xC1,
        Nop_Immediate_C2 = 0xC2, // Illegal
        Dcp_IndirectZeroPageWithXOffset_C3 = 0xC3, // Illegal
        Cpy_ZeroPage = 0xC4,
        Cmp_ZeroPage = 0xC5,
        Dec_ZeroPage = 0xC6,
        Dcp_ZeroPage_C7 = 0xC7, // Illegal
        Iny_Implied = 0xC8,
        Cmp_Immediate = 0xC9,
        Dex_Implied = 0xCA,
        Axs_Immediate_CB = 0xCB, // Illegal
        Cpy_Absolute = 0xCC,
        Cmp_Absolute = 0xCD,
        Dec_Absolute = 0xCE,
        Dcp_Absolute_CF = 0xCF, // Illegal
        Bne_Relative = 0xD0,
        Cmp_IndirectZeroPageWithYOffset = 0xD1,
        Kil_Implied_D2 = 0xD2, // Illegal
        Dcp_IndirectZeroPageWithYOffset_D3 = 0xD3, // Illegal
        Nop_ZeroPageWithXOffset_D4 = 0xD4, // Illegal
        Cmp_ZeroPageWithXOffset = 0xD5,
        Dec_ZeroPageWithXOffset = 0xD6,
        Dcp_ZeroPageWithXOffset_D7 = 0xD7, // Illegal
        Cld_Implied = 0xD8,
        Cmp_AbsoluteWithYOffset = 0xD9,
        Nop_Implied_DA = 0xDA, // Illegal
        Dcp_AbsoluteWithYOffset_DB = 0xDB, // Illegal
        Nop_AbsoluteWithXOffset_DC = 0xDC, // Illegal
        Cmp_AbsoluteWithXOffset = 0xDD,
        Dec_AbsoluteWithXOffset = 0xDE,
        Dcp_AbsoluteWithXOffset_DF = 0xDF, // Illegal
        Cpx_Immediate = 0xE0,
        Sbc_IndirectZeroPageWithXOffset = 0xE1,
        Nop_Immediate_E2 = 0xE2, // Illegal
        Isc_IndirectZeroPageWithXOffset_E3 = 0xE3, // Illegal
        Cpx_ZeroPage = 0xE4,
        Sbc_ZeroPage = 0xE5,
        Inc_ZeroPage = 0xE6,
        Isc_ZeroPage_E7 = 0xE7, // Illegal
        Inx_Implied = 0xE8,
        Sbc_Immediate = 0xE9,
        Nop_Implied = 0xEA,
        Sbc_Immediate_EB = 0xEB, // Illegal
        Cpx_Absolute = 0xEC,
        Sbc_Absolute = 0xED,
        Inc_Absolute = 0xEE,
        Isc_Absolute_EF = 0xEF, // Illegal
        Beq_Relative = 0xF0,
        Sbc_IndirectZeroPageWithYOffset = 0xF1,
        Kil_Implied_F2 = 0xF2, // Illegal
        Isc_IndirectZeroPageWithYOffset_F3 = 0xF3, // Illegal
        Nop_ZeroPageWithXOffset_F4 = 0xF4, // Illegal
        Sbc_ZeroPageWithXOffset = 0xF5,
        Inc_ZeroPageWithXOffset = 0xF6,
        Isc_ZeroPageWithXOffset_F7 = 0xF7, // Illegal
        Sed_Implied = 0xF8,
        Sbc_AbsoluteWithYOffset = 0xF9,
        Nop_Implied_FA = 0xFA, // Illegal
        Isc_AbsoluteWithYOffset_FB = 0xFB, // Illegal
        Nop_AbsoluteWithXOffset_FC = 0xFC, // Illegal
        Sbc_AbsoluteWithXOffset = 0xFD,
        Inc_AbsoluteWithXOffset = 0xFE,
        Isc_AbsoluteWithXOffset_FF = 0xFF, // Illegal
    }
}